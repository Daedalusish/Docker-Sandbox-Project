# Docker Sandbox

Run coding agents in **slots**: each slot is its own git clone in its own dev container.
An agent can break its slot, but not your repo, your credentials or other slots.

```
frontend/       Node + Vite example
backend/        .NET API + Postgres example, with tests
.devcontainer/  one config per kind of agent
presentation/   notes
```

## One-time setup

1. Install git, make, a container engine (Docker Desktop or Podman) and [DevPod](https://devpod.sh).
   Windows: run everything from Git Bash.
2. Point DevPod at your engine: `devpod provider add docker`
   (Podman: `devpod provider set-options docker -o DOCKER_PATH=podman`).
3. Stop DevPod handing your credentials to the agents. It's on by default: your SSH keys, git logins and
   registry logins would be usable inside every slot. Check with `devpod context options` afterwards.
   ```bash
   devpod context set-options \
     -o SSH_AGENT_FORWARDING=false -o SSH_ADD_PRIVATE_KEYS=false \
     -o SSH_INJECT_GIT_CREDENTIALS=false -o SSH_INJECT_DOCKER_CREDENTIALS=false \
     -o GIT_SSH_SIGNATURE_FORWARDING=false
   ```
4. **Linux with rootless Podman:** add `userns = "keep-id"` under `[containers]`
   in `~/.config/containers/containers.conf`.
5. **Linux with SELinux (Fedora, Bazzite, …):** let containers read the repo, once:
   `mkdir -p slots && chcon -R -t container_file_t .git slots`

## Commands

Run `make` to list them. Each one is a few plain git and devpod commands; see the `Makefile`.

```bash
make frontend-agent name=alpha                          # create and start slot "frontend-alpha"
make fetch name=frontend-alpha branch=agent/dark-mode   # copy its branch into this repo
git log -p main..agent/dark-mode                        # review, then merge and push from here
make remove name=frontend-alpha                         # delete the slot and its container
```

Slots only contain committed work. Backend agents start the database themselves.

## Three kinds of agent

### Normal: `frontend-agent`, `backend-agent`

A clone of this repo, plus a read-only view of it, so the slot can `git fetch` and switch branches itself.

- **Benefits:** works as a long-lived agent; can't write to this repo; has its own git identity.
- **Cons:** can read all of this repo's `.git`, including other branches and `.git/config`.
  Work comes in through one `make fetch`.
- **Requires:** no credentials in `.git/config`. Use a credential manager, not a token in the remote URL.

### Strict: `frontend-agent-strict`, `backend-agent-strict`

A clone of this repo and nothing else.

- **Benefits:** the smallest exposure: it sees only its own clone. Same way out (`make fetch`), own git identity.
- **Cons:** can't fetch. New branches only arrive as bundles, two steps started from your machine.
  Clumsy for long-lived agents.
- **Requires:** nothing extra. To update it: `make bundle name=…`, then inside the slot
  `git fetch /workspace/.git/main.bundle 'refs/heads/*:refs/remotes/origin/*'`.

### Worktree: `frontend-agent-worktree`, `backend-agent-worktree`

The old way, for comparison: a git worktree that shares this repo's `.git`, mounted read-write.

- **Benefits:** no fetch: commits on `agent/NAME` land here straight away. Least disk.
- **Cons:** the agent can rewrite any branch, the git config and the hooks, and hooks run on your machine.
  It shares your git config, so it has no git identity of its own. Fragile pointer files.
- **Requires:** trusting the agent with the whole repo. If git in the slot complains about
  "dubious ownership", run `git config --global --add safe.directory /workspace` there.

## Working in a slot

- **Task slot:** one branch (`git switch -c agent/alpha` inside the slot), removed when done.
- **Long-lived slot:** kept; inside it, `git fetch && git switch <branch>` as needed.
- Check work inside the slot: `dotnet test`, or `npm run lint` and `npm run typecheck`.

**Never run git inside a slot from your machine.** The agent controls that slot's hooks and config.
`make fetch` only reads from the slot and doesn't run its hooks.

## Known issues

- **Rider shows nothing on a new backend slot's first start.** The solution loads, but the
  Solution Explorer stays empty: a timing bug in Rider's remote mode (seen in 2026.2.3.1).
  Restart the slot once and it works from then on:
  `make stop name=backend-NAME`, then `make start name=backend-NAME`.

## What this does not protect

The network is open, containers share your machine's kernel, and backend slots share one database.
[Docker Sandboxes](https://docs.docker.com/ai/sandboxes/) (`sbx`) adds a VM and network rules on top.

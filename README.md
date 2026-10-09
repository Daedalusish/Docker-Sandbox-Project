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
3. Turn off DevPod's git credential and SSH agent forwarding (`devpod context options`).
4. **Linux with rootless Podman only:**
   - add `userns = "keep-id"` under `[containers]` in `~/.config/containers/containers.conf`
   - with SELinux, let containers use the repo: `chcon -R -t container_file_t .git slots`

## Commands

Run `make` to list them. Each one is a few plain git and devpod commands; see the `Makefile`.

```bash
make frontend-agent name=alpha                          # create and start slot "frontend-alpha"
make fetch name=frontend-alpha branch=agent/dark-mode   # copy its branch into this repo
git log -p main..agent/dark-mode                        # review, then merge and push from here
make remove name=frontend-alpha                         # delete the slot and its container
```

Backend agents (`make backend-agent name=…`) need the database first: `make db-up` (Podman: `ENGINE=podman`).
Slots only contain committed work.

| Command | The slot can… |
|---|---|
| `frontend-agent`, `backend-agent` | read the main repo, so it can `git fetch` and switch branches itself |
| `frontend-agent-strict`, `backend-agent-strict` | see nothing but its own clone |

- **Task slot:** one branch (`git switch -c agent/alpha` inside the slot), removed when done.
- **Long-lived slot:** kept; inside it, `git fetch && git switch <branch>` as needed.
  Strict slots can't fetch: run `make bundle name=…`, then inside the slot
  `git fetch .git/main.bundle 'refs/heads/*:refs/remotes/origin/*'`.
- Check work inside the slot: `dotnet test`, or `npm run lint` and `npm run typecheck`.

**Never run git inside a slot from your machine.** The agent controls that slot's hooks and config.
`make fetch` only reads from the slot and doesn't run its hooks.

## What this does not protect

The network is open, containers share your machine's kernel, and backend slots share one database.
[Docker Sandboxes](https://docs.docker.com/ai/sandboxes/) (`sbx`) adds a VM and network rules on top.

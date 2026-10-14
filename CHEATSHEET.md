# Cheat sheet

`HOST` = your machine, from the repo root. `SLOT` = the terminal inside the agent's container.
Slot names are `frontend-NAME` or `backend-NAME`.

## Once per machine

```bash
# HOST: stop DevPod handing your SSH keys and logins to the agents (on by default)
devpod context set-options \
  -o SSH_AGENT_FORWARDING=false -o SSH_ADD_PRIVATE_KEYS=false \
  -o SSH_INJECT_GIT_CREDENTIALS=false -o SSH_INJECT_DOCKER_CREDENTIALS=false \
  -o GIT_SSH_SIGNATURE_FORWARDING=false
devpod context options                   # HOST: check, VALUE should say false
```

Full setup (engine, DevPod, Podman, SELinux): see the README.

## Always first

```bash
git add -A && git commit -m "…"          # HOST: slots copy your last commit, not your files
```

## The flow, per mode

### Normal: `frontend-agent`, `backend-agent`

```bash
make backend-agent name=alpha                          # HOST: create slot backend-alpha
git fetch                                              # SLOT: see main's latest
git switch -c agent/my-task origin/main                # SLOT: new task branch
git add . && git commit -m "…"                         # SLOT
make fetch name=backend-alpha branch=agent/my-task     # HOST: copy the branch here
```

### Strict: `frontend-agent-strict`, `backend-agent-strict`

Same as normal, except the slot can't fetch. Hand it main's latest instead:

```bash
make backend-agent-strict name=alpha                                        # HOST
make bundle name=backend-alpha                                              # HOST: when the slot needs updates
git fetch /workspace/.git/main.bundle 'refs/heads/*:refs/remotes/origin/*'  # SLOT
git switch -c agent/my-task origin/main                                     # SLOT
git add . && git commit -m "…"                                              # SLOT
make fetch name=backend-alpha branch=agent/my-task                          # HOST
```

### Worktree: `frontend-agent-worktree`, `backend-agent-worktree`

Shares this repo's history: no fetch in either direction. Starts on branch `agent/NAME`.

```bash
make backend-agent-worktree name=alpha                 # HOST: slot on branch agent/alpha
git switch -c agent/my-task main                       # SLOT: next task (main, not origin/main)
git add . && git commit -m "…"                         # SLOT: lands in this repo at once
```

## Then, for every mode

```bash
git log -p main..agent/my-task                         # HOST: review
git merge agent/my-task && git push                    # HOST: push only from here
```

Never run git inside `slots/…` from the HOST.

## Frontend vs backend

| | Frontend | Backend |
|---|---|---|
| Opens in | VS Code | Rider |
| Checks, in the SLOT | `npm run lint && npm run typecheck` | `dotnet test` |
| Database | none | shared, started by `make backend-agent…` |
| First start | works | Rider may show nothing: `make stop` + `make start` once |

## Manage slots (all modes)

```bash
make list                                 # HOST: all slots and their state
make stop name=backend-alpha              # HOST: stop, keep everything
make start name=backend-alpha             # HOST: start again
make remove name=backend-alpha            # HOST: delete slot and container
claude                                    # SLOT: start Claude, first time /login
```

## Database (backend only)

```bash
make db-up                                # HOST: start (backend agents do this themselves)
make db-down                              # HOST: stop, data is kept
podman exec sandbox-demo-db psql -U postgres -c 'select * from history order by id desc limit 5'
```

## When it breaks

| Symptom | Applies to | Fix |
|---|---|---|
| Slot has old files or config | all | You didn't commit. Commit, `make remove`, create again |
| `make` stops at once | all | You forgot `name=` |
| `Permission denied` inside the slot | all, Linux with SELinux | `mkdir -p slots && chcon -R -t container_file_t .git slots` |
| `network sandbox-demo not found` | backend | `make db-up` |
| Rider connected but empty | backend | `make stop` + `make start` on that slot |
| `git fetch` fails in the slot | strict | Expected: use `make bundle` |
| Git complains about "dubious ownership" | worktree | SLOT: `git config --global --add safe.directory /workspace` |
| `switch -c … origin/main` gives an old main | worktree | Use `main`: the worktree shares this repo's branches |

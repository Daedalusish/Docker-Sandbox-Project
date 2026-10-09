# Run `make` to see the commands.

# Use ENGINE=podman if you use Podman
ENGINE ?= docker

help:
	@echo "Database"
	@echo "  make db-up                               start the shared database"
	@echo "  make db-down                             stop it"
	@echo "Agents (each creates a slot called frontend-NAME or backend-NAME)"
	@echo "  make frontend-agent name=NAME            frontend agent"
	@echo "  make backend-agent name=NAME             backend agent"
	@echo "  make frontend-agent-strict name=NAME     frontend agent that can't see the main repo"
	@echo "  make backend-agent-strict name=NAME      backend agent that can't see the main repo"
	@echo "Slots"
	@echo "  make list                                show all slots"
	@echo "  make start name=SLOT                     start a stopped slot"
	@echo "  make stop name=SLOT                      stop a slot, keep everything"
	@echo "  make remove name=SLOT                    delete a slot"
	@echo "Moving work"
	@echo "  make fetch name=SLOT branch=BRANCH       copy a slot's branch into this repo"
	@echo "  make bundle name=SLOT                    give a strict slot the latest branches"


# ---- Database (needed by backend agents) ----

# Start the shared database
db-up:
	cd backend && $(ENGINE) compose up -d

# Stop the shared database. Its data is kept.
db-down:
	cd backend && $(ENGINE) compose down


# ---- Agents ----

# Create and start a frontend agent: make frontend-agent name=alpha
frontend-agent:
	git clone . slots/frontend-$(name)
	git -C slots/frontend-$(name) remote set-url origin /main.git
	git -C slots/frontend-$(name) config user.name "Claude (frontend-$(name))"
	devpod up ./slots/frontend-$(name) --devcontainer-path .devcontainer/frontend/devcontainer.json

# Create and start a backend agent: make backend-agent name=alpha
backend-agent:
	git clone . slots/backend-$(name)
	git -C slots/backend-$(name) remote set-url origin /main.git
	git -C slots/backend-$(name) config user.name "Claude (backend-$(name))"
	devpod up ./slots/backend-$(name) --devcontainer-path .devcontainer/backend/devcontainer.json

# Like frontend-agent, but the slot can't see the main repo
frontend-agent-strict:
	git clone . slots/frontend-$(name)
	git -C slots/frontend-$(name) config user.name "Claude (frontend-$(name))"
	devpod up ./slots/frontend-$(name) --devcontainer-path .devcontainer/frontend-strict/devcontainer.json

# Like backend-agent, but the slot can't see the main repo
backend-agent-strict:
	git clone . slots/backend-$(name)
	git -C slots/backend-$(name) config user.name "Claude (backend-$(name))"
	devpod up ./slots/backend-$(name) --devcontainer-path .devcontainer/backend-strict/devcontainer.json


# ---- Slots (use the full slot name, e.g. frontend-alpha) ----

# Show all slots and whether they are running
list:
	devpod list

# Start a stopped slot: make start name=frontend-alpha
start:
	devpod up $(name)

# Stop a slot. Everything in it is kept: make stop name=frontend-alpha
stop:
	devpod stop $(name)

# Delete a slot and its container: make remove name=frontend-alpha
# The first line stops if name is missing, so rm can't delete every slot.
remove:
	@test -n "$(name)"
	devpod delete $(name)
	rm -rf slots/$(name)


# ---- Moving work in and out ----

# Copy a slot's branch into this repo: make fetch name=frontend-alpha branch=agent/dark-mode
fetch:
	git fetch ./slots/$(name) $(branch):$(branch)

# Give a strict slot the latest branches: make bundle name=frontend-alpha
# Then, inside the slot: git fetch .git/main.bundle 'refs/heads/*:refs/remotes/origin/*'
bundle:
	git bundle create slots/$(name)/.git/main.bundle --all

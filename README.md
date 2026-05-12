# UNSEEN

Initial repository setup for the UNSEEN Unity VR project.

## Discord Notification Setup

1. Create a Discord webhook in the target channel.
2. In GitHub, open `Settings > Secrets and variables > Actions`.
3. Add a repository secret named `DISCORD_WEBHOOK_URL`.
4. Push this repository to GitHub.

After that, Discord notifications will trigger for:

- pushes to `main`
- pushes to `develop`
- pull request opened / updated / reopened
- pull request merged

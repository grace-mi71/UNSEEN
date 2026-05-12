# Discord Integration

This repository is prepared to send GitHub updates to Discord by webhook.

## What It Sends

- push notifications for `main`
- push notifications for `develop`
- pull request opened
- pull request reopened
- pull request updated
- pull request merged

## Discord Setup

1. Open your Discord server.
2. Go to the target text channel.
3. Open `Edit Channel > Integrations > Webhooks`.
4. Create a webhook.
5. Copy the webhook URL.

## GitHub Setup

1. Open the repository: `https://github.com/grace-mi71/UNSEEN`
2. Go to `Settings > Secrets and variables > Actions`
3. Create a new repository secret:

   - Name: `DISCORD_WEBHOOK_URL`
   - Value: your Discord webhook URL

## Workflow File

The notification workflow is stored at:

- `.github/workflows/discord-notify.yml`

## Recommended Discord Channels

- `#announcements`
- `#dev-log`
- `#build-share`
- `#bugs`

## Suggested Next Step

After Unity project files are added, make the first push so the webhook can be tested with a real repository event.

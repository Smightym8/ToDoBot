# Discord ToDoBot 
I wrote this bot in order to remind me about my university assignments which are provided by software that has a built in calendar with a link to integrate the assignments in an own calendar. This bot can use an ics calendar link to fetch the calendar entries and notifies you on discord about the assignments for the next 7 days. 

# Usage

Please lookup how to create a discord bot and to get a token for it. This bot needs a token from discord, a link to an ics calendar and the channel id of the channel where your want to get your reminder.

Organize the bot token, ics link and channel id in a .env file like this:

```
TOKEN=[Your bot token]
CHANNEL_ID=[Id of the target channel]
CALENDAR_LINK=[Link of the ics calendar]
```

Clone the repo and put the python file together with the requirements.txt on a Linux machine e.g. a Raspberry Pi.
Install the packages with: 

```bash
pip install -r requirements.txt
```

After this you can run the python file and the bot should be shown as online on your discord server.

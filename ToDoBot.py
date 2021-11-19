import os

from dotenv import load_dotenv
import discord
from icalendar import Calendar
from datetime import datetime, date
import requests
from discord.ext import tasks, commands

# Defines the structure of an element from the calendar
# and which attributes are the keys of the object
class ToDo(object):
    def __init__(self, description: str, date: str, time: str):
        self.description = description
        self.date = date
        self.time = time

    def __hash__(self):
        return hash(('description', self.description,
                     'date', self.date))

    def __eq__(self, other):
        return self.description == other.description \
               and self.date == other.date

# Takes the url to the ics calendar
def fetch_dates(url: str):
    events = requests.get(url).text
    gcal = Calendar.from_ical(events)
    todo_items = [] # List to store the items from the calendar
    for component in gcal.walk():
        if component.name == "VEVENT":
            summary = component.get('summary')
            start = component.get('dtstart').dt
            timestamp = str(start.time())
            date_string = str(start.day) + "." + str(start.month) + "." + str(start.year)

            if isinstance(start, datetime):
                start = datetime.date(start) # Parse datetime objects to date

            date_difference = (start - date.today()).days # Get difference between today and deadline

            if 0 <= date_difference <= 7:
                todo = ToDo(summary, date_string, timestamp)
                todo_items.append(todo)

    # Remove duplicates and sort todo_items
    todo_items = list(set(todo_items))
    todo_items.sort(key=lambda x: x.date, reverse=False)
    return todo_items

#Runs everyday
@tasks.loop(hours=24)
async def send_todos():
    today = date.today().weekday()
    # 0 = Monday, 1 = Tuesday, 2 = Wednesday, 3 = Thursday, ...
    if today == 0 or today == 2 or today == 4:
        message_channel = bot.get_channel(int(target_channel_id))

        # Fetch calendar items
        items = fetch_dates(str(ILIAS_LINK))

        # Check if there are any items
        if len(items) < 1:
            embed = discord.Embed(
                title="Nothing to do for the next 7 days",
                color=discord.Color.blue()
            )
            await message_channel.send(embed=embed)
        else:
            todo_embed = discord.Embed(
                title="ToDos for the next 7 days",
                color=discord.Color.blue()
            )

            # Iterate over items and add them to embed
            for item in items:
                # Create fancy embed field for each todo
                todo_embed.add_field(name="Assignment", value=item.description)
                todo_embed.add_field(name="Submission date", value=item.date)
                todo_embed.add_field(name="Submission time", value=item.time)

            await message_channel.send(embed=todo_embed)

# Called first and let's sleep the program until bot is ready
@send_todos.before_loop
async def before():
    await bot.wait_until_ready()

# Main method of ToDoBot.py
if __name__ == '__main__':
    load_dotenv()
    TOKEN = os.getenv('TOKEN')
    CHANNEL_ID = os.getenv('CHANNEL_ID') # Use specific channel from discord server
    ILIAS_LINK = os.getenv('CALENDAR_LINK')

    bot = commands.Bot(command_prefix='!', description="This is a Discord Bot to remind you about your todos")
    target_channel_id = CHANNEL_ID
    send_todos.start()
    bot.run(TOKEN)

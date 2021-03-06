# Numerijeweld

## General Information

## The game itself is divided into 3 "scenes"

### Main Menu

At the start the user will be prompted to input A "User code", a random generated code unique to each player.

Using that code we retrieve the [user information][#User Information] from the server and then the rest of the main menu will show.

Note- if the player already ran the game before, the user information was saved on the device and it will be loaded the next time the player starts the game.

The rest of the main menu has 2 options

#### Start Game

Will move the player to the next scene which is the [tutorial scene][#Tutorial].

#### Play Statistics

Will show the player his available play dates,how many sessions he has each day,the length of each session and the remaining time for the current session.

Some days will have a status symbol next to them- a checkmark indicated that all of that days play sessions were completed while A cross indicates that not all play sessions of that day were completed and so the player cannot continue playing.

### Tutorial

This scene contains an instance of the game yet you can't lose and play time does not count towards your session play time. This is a sort of sandbox that you can use to learn the game before going in to the game for the first time.

At the bottom of the tutorial screen there's a button that's used to skip to the main game

### Main Game

This is the main scene of the application. the game board is identical to the one in the [tutorial][#Tutorial] yet here there's a timer and game score. everything that happens here counts toward the player score and play time.

#### Levels

Starting at level 0, this determines the distance between each 2 numbers in a numerical series you need to make to get scored. At level 0 you need to make a series of 3 or more identical numbers. after a certain amount of points the player will level up and the distance between numbers will increase by 1. However, at all levels above 0 you can still do a series of identical numbers yet you will not be scored. this is so it's easier to clear the board.

For example-

​at level 0- [4,4,4] :heavy_check_mark: is a series while [4,5,6] :negative_squared_cross_mark: and [4,6,8] :negative_squared_cross_mark: are not a series 

​at level 1- [4,4,4] :black_circle: is a series that worth no points,[4,5,6] :heavy_check_mark: is a series and [4,6,8] :negative_squared_cross_mark: is not a series

​at level 2- [4,4,4] :black_circle: is a series that worth no points.[4,5,6] :negative_squared_cross_mark: is not a series and [4,6,8] :heavy_check_mark: is a series 

and so on

#### Score

Points are awarded based on the numbers in the series the player made, for any series (except the ones that worth no points explained [above][#Levels]) the amount of points the player gets is equal to the sum of the numbers that make the series

The Player will lose 5 points for every move that he makes that does not create a series

For example-

for the series [7,7,7] **at level 0** the player will get 7+7+7=21 points. at any other level the player will get **0** points

for the series [1,2,3] the player will get 1+2+3=6 points *only at level 1 where that series is legal*

for the series [2,4,6] the player will get 2+4+6=12 points *only at level 2 where that series if legal*

#### Timer

When this timer reaches 0 the game will end.For every match the player makes he gets an additional 5 seconds to the timer. The timer is limited to 100 seconds maximum.

#### Game End

The game can end in a number of different ways

1. The general timer will reach 0
2. The player did not make a move for 10 seconds
3. Having a negative amount of points (by making a move that does not make series as the first move, for example)

## Technical Information

### IControllerInterface

Acts as a base class for all the scene controllers. This givs the Game object the ability to exist in any scene easily. The different controllers handle all the button callbacks and in charge of UI animations and text

###Game
This class is attached to the game board object in the game. it handles all the game logic,mouse input and animations.

###ShapeMatrix
Contains a matrx used to store the number cells for the game. Contains all the search and matching algorithms 

### User Information

#### UserLocalData class

This class symbolizes the data saved to the device. an instance of the class is serialized in JSON format and saved locally.

it contains the user code and an array of the PlayDate class

#### PlayDate class

Contains data on each date the player can play in.  

The play dates are received as a JSON formatted text from the server and has the following structure

```json
[
  {
    "session_id": 123,
    "date": "yyyy-MM-dd",
    "sessions": 123,
    "session_length": 123,
    "session_interval": 123
  }
]
```

for each item in the JSON array (there could be any number of them) we have the following parameters:

* **session_id** - A unique id related to each date and player

- **date** - Date in which the sessions are played
- **sessions** - The number of sessions this day
- **session_length** - The length (in minutes) of each session
- **session_interval** - The time (in minutes) the player has to wait between sessions  

Here is an example of a JSON containing 3 play dates (15/3/17,16/3/17,17/3/17)

each day has 3 sessions and each sessions is 20 minutes long with an hour interval between them. 

```json
[
  {
    "session_id": 1,
    "date": "2017-03-15",
    "sessions": 3,
    "session_length": 20,
    "session_interval": 60
  },
    {
    "session_id": 5,
    "date": "2017-03-16",
    "sessions": 3,
    "session_length": 20,
    "session_interval": 60
  },
    {
    "session_id": 8,
    "date": "2017-03-17",
    "sessions": 3,
    "session_length": 20,
    "session_interval": 60
  }
]
```

In additions to the data from the JSON we have more fields in the class

##### GameRuns

An array of Runs classes symbolizing one continuous game until either the player lost or the session ended 

##### CurrentSession

The number of the session the player is playing on now

##### CurrentSessionTime

Total time played on the CurrentSession

##### LastSessionsEndTime

A timestamp of when the previous session was finished. Used to check the interval between sessions

#### Score reporting

For every move in the game we save the score delta caused by that move (positive for matches or negative for just swaps)

We save those in a list of instances of the ScoreReports class which is identical to the JSON [sent][#POST scores]

We report to the server every 10 seconds 

### API Calls

We have 2 main API calls in the game

#### GET user information

* GET

* /game/:code_id

* Returns an array of [play dates][#PlayDate class]

  ​

#### POST scores

* POST

* /game/:code_id/:session_id

  ```json
  [
    {
      "score": int,
      "timestamp": string,
      "session_id": int
    }
  ]
  ```






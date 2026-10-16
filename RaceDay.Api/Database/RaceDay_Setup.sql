/*
  RaceDay development schema
  BACKS UP my current database before using this script.
  This script creates a NEW database named Raceday_Dev so it does not overwrite my existing Raceday DB.
*/
CREATE DATABASE Raceday_Dev;
GO
USE Raceday_Dev;
GO

CREATE TABLE Users
(
    userID INT IDENTITY(1,1) PRIMARY KEY,
    fullName VARCHAR(100) NOT NULL,
    emailAddress VARCHAR(100) NOT NULL UNIQUE,
    phoneNumber VARCHAR(20) NOT NULL,
    password VARCHAR(100) NOT NULL,
    userRole VARCHAR(50) NOT NULL
);
GO

CREATE TABLE Organisers
(
    organiserID INT IDENTITY(1,1) PRIMARY KEY,
    userID INT NOT NULL UNIQUE,
    CONSTRAINT FK_Organisers_Users FOREIGN KEY (userID) REFERENCES Users(userID)
);
GO

CREATE TABLE Events
(
    eventID INT IDENTITY(1,1) PRIMARY KEY,
    eventName VARCHAR(100) NOT NULL,
    description VARCHAR(255) NOT NULL,
    eventDate DATE NOT NULL,
    location VARCHAR(100) NOT NULL,
    distance DECIMAL(5,2) NOT NULL,
    eventType VARCHAR(50) NOT NULL,
    organiserID INT NOT NULL,
    CONSTRAINT FK_Events_Organisers FOREIGN KEY (organiserID) REFERENCES Organisers(organiserID)
);
GO

CREATE TABLE Participants
(
    participantID INT IDENTITY(1,1) PRIMARY KEY,
    userID INT NOT NULL UNIQUE,
    CONSTRAINT FK_Participants_Users FOREIGN KEY (userID) REFERENCES Users(userID)
);
GO

CREATE TABLE Categories
(
    categoryID INT IDENTITY(1,1) PRIMARY KEY,
    categoryName VARCHAR(100) NOT NULL,
    distance DECIMAL(5,2) NOT NULL,
    age INT NOT NULL,
    startTime TIME NOT NULL,
    eventID INT NOT NULL,
    organiserID INT NOT NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (eventID) REFERENCES Events(eventID),
    CONSTRAINT FK_Categories_Organisers FOREIGN KEY (organiserID) REFERENCES Organisers(organiserID)
);
GO

CREATE TABLE Enrolments
(
    enrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    enrolmentDate DATE NOT NULL DEFAULT GETDATE(),
    eventID INT NOT NULL,
    participantID INT NOT NULL,
    categoryID INT NOT NULL,
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (eventID) REFERENCES Events(eventID),
    CONSTRAINT FK_Enrolments_Participants FOREIGN KEY (participantID) REFERENCES Participants(participantID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (categoryID) REFERENCES Categories(categoryID),
    CONSTRAINT UQ_Enrolments_Event_Participant UNIQUE (eventID, participantID)
);
GO

CREATE TABLE Results
(
    resultID INT IDENTITY(1,1) PRIMARY KEY,
    finishTime TIME NOT NULL,
    finishPosition INT NOT NULL,
    enrolmentID INT NOT NULL UNIQUE,
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (enrolmentID) REFERENCES Enrolments(enrolmentID)
);
GO

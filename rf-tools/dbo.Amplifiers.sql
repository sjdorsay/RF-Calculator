CREATE TABLE [dbo].[Amplifiers]
(
	[Name] NVARCHAR(50) NOT NULL PRIMARY KEY, 
    [Gain] FLOAT NOT NULL, 
    [Frequency] FLOAT NOT NULL, 
    [Bandwidth] FLOAT NOT NULL
)

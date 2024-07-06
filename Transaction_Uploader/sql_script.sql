-- db_aa4116_transaction.dbo.[Transaction] definition

-- Drop table

-- DROP TABLE db_aa4116_transaction.dbo.[Transaction];

CREATE TABLE db_aa4116_transaction.dbo.[Transaction] (
	Transaction_Id varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Amount decimal(38,2) NOT NULL,
	Currency_Code char(3) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	Transaction_Date datetime NOT NULL,
	Status char(1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	CONSTRAINT Transaction_PK PRIMARY KEY (Transaction_Id)
);
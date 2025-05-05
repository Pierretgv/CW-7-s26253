-- foreign keys
ALTER TABLE Client_Trip DROP CONSTRAINT FK_Client_Trip_ClientId;
ALTER TABLE Client_Trip DROP CONSTRAINT FK_Client_Trip_TripId;

-- tables
DROP TABLE Client_Trip;
DROP TABLE Client;
DROP TABLE Trip;







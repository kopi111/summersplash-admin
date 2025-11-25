-- Add some dummy locations
INSERT INTO JobLocations (Name, Address, IsActive, CreatedAt)
VALUES
('Main Office', '123 Main St', 1, GETDATE()),
('Downtown Pool', '456 Downtown Ave', 1, GETDATE()),
('Westside Recreation Center', '789 West Blvd', 1, GETDATE()),
('Eastside Aquatic Center', '321 East Dr', 1, GETDATE()),
('North Community Pool', '654 North Rd', 1, GETDATE());

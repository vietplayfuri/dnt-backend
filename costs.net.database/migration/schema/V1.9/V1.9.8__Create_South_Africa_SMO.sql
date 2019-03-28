INSERT INTO smo(id, region_id, "key", "value")
  VALUES (uuid_generate_v4(), 
  (SELECT id FROM region WHERE "name" = 'IMEA'),
  'SOUTHERN AFRICA CLUSTER',
  'SOUTHERN AFRICA CLUSTER');

UPDATE smo
  SET "key" = 'EAST WEST WHITESPACE AFRICA',
    "value" = 'EAST WEST WHITESPACE AFRICA'
  WHERE "key" = 'SOUTH AFRICA  & SSA EXPANSION MARKETS GROUP';

UPDATE country
  SET smo_id = (SELECT id FROM smo WHERE "key" = 'SOUTHERN AFRICA CLUSTER')
  WHERE iso IN ('NA', 'MZ', 'ZA', 'SZ', 'ZM', 'LS', 'BW', 'AO', 'ZW');
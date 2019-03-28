UPDATE country
  SET smo_id = (SELECT id FROM smo WHERE "key" = 'EAST WEST WHITESPACE AFRICA')
  WHERE iso = 'NG';
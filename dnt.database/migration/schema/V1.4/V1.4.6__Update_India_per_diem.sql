UPDATE per_diem
  SET region = 'IMEA'
  WHERE lower(country) = 'india' AND lower(region) = 'aak';
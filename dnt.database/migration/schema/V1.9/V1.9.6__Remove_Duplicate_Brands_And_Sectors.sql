--create sector map
SELECT s.id AS new_id, s_replace.id AS old_id, s."name"
  INTO sector_select 
  FROM sector s
  INNER JOIN sector s_replace ON s."name" = s_replace."name" AND s.created < s_replace.created;

--wipe duplicate sectors from brands
UPDATE brand b
  SET sector_id = (SELECT ss.new_id FROM sector_select ss WHERE ss.old_id = b.sector_id),
  modified = now()
  WHERE b.sector_id IN (SELECT ss.old_id FROM sector_select ss);

--remove unused sectors (duplicates)
DELETE FROM sector s
  WHERE s.id IN (SELECT ss.old_id FROM sector_select ss);

--create brand map
SELECT b.id AS new_id, b_replace.id AS old_id, b."name"
  INTO brand_select
  FROM brand b
  INNER JOIN brand b_replace ON b."name" = b_replace."name" AND b.sector_id = b_replace.sector_id AND b.created < b_replace.created;

--wipe duplicate brands from projects
UPDATE project p
  SET brand_id = (SELECT bs.new_id FROM brand_select bs WHERE bs.old_id = p.brand_id),
  modified = now()
  WHERE p.brand_id IN (SELECT bs.old_id FROM brand_select bs);

--remove unused brands (duplicates)
DELETE FROM brand b
  WHERE b.id IN (SELECT bs.old_id FROM brand_select bs);


--clean up temp tables
DROP TABLE sector_select;
DROP TABLE brand_select;

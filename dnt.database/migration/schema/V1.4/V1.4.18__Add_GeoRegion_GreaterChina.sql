
insert into geo_region ("key", value)
SELECT 'Greater China', 'Greater China'
WHERE
    NOT EXISTS (
        SELECT id FROM geo_region WHERE "key" = 'Greater China'
    );

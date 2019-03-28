alter table brand 
    add sector_id uuid null,
    add constraint brand_sector_fk foreign key (sector_id) references sector(id);

alter table brand drop column agency_id;

CREATE INDEX brand_name_idx ON brand(name);
CREATE INDEX sector_name_idx ON sector(name);
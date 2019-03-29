
-- AAK, All Other Locations, All Other Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('AAK', 'All Other Locations', 'All Other Locations', false, 200, '2018-03-12');

-- Europe, All Other Locations, All Other Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Europe', 'All Other Locations', 'All Other Locations', false, 300, '2018-03-12');

-- IMEA, All Other Locations, All Other Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('IMEA', 'All Other Locations', 'All Other Locations', false, 300, '2018-03-12');

-- North America, All Other Locations, All Other Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('North America', 'All Other Locations', 'All Other Locations', false, 300, '2018-03-12');

-- AAK, Australia, All Other Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('AAK', 'Australia', 'All Other Locations', false, 350, '2018-03-12');

-- Latin America, Brazil, Sao Paulo
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Latin America', 'Brazil', 'Sao Paulo', false, 350, '2018-03-12');

-- Latin America, Brazil, Río de Janeiro
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Latin America', 'Brazil', 'Río de Janeiro', false, 350, '2018-03-12');

-- North America, Canada, Vancouver
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('North America', 'Canada', 'Vancouver', false, 450, '2018-03-12');

-- Europe, France, Paris
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Europe', 'France', 'Paris', false, 350, '2018-03-12');

-- Greater China, All Other Locations, All Other Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Greater China', 'All Other Locations', 'All Other Locations', false, 300, '2018-03-12');

-- IMEA, India, Mumbai
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('IMEA', 'India', 'Mumbai', false, 350, '2018-03-12');

-- IMEA, India, All Other Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('IMEA', 'India', 'All Other Locations', false, 300, '2018-03-12');

-- Latin America, Mexico, Mexico City
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Latin America', 'Mexico', 'Mexico City', false, 350, '2018-03-12');

-- IMEA, Nigeria, All Locations
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('IMEA', 'Nigeria', 'All Locations', false, 350, '2018-03-12');

-- Europe, Russia, Moscow
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Europe', 'Russia', 'Moscow', false, 350, '2018-03-12');

-- Europe, United Kingdom, London
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Europe', 'United Kingdom', 'London', false, 350, '2018-03-12');

-- North America, United States, Los Angeles
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('North America', 'United States', 'Los Angeles', false, 450, '2018-03-12');

-- North America, United States, San Francisco
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('North America', 'United States', 'San Francisco', false, 450, '2018-03-12');

-- North America, United States, New York City
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('North America', 'United States', 'New York City', false, 450, '2018-03-12');

-- North America, United States, Chicago
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('North America', 'United States', 'Chicago', false, 450, '2018-03-12');

-- Latin America, Venezuela, Caracas
insert into per_diem (region, country, shoot_city, is_default, cost, effective_from)
values('Latin America', 'Venezuela', 'Caracas', false, 350, '2018-03-12');

update per_diem
set shoot_city = 'All Locations'
where region = 'AAK' and country in ('Australia', 'Indonesia', 'Korea, Republic of', 'Malaysia', 'New Zealand', 'Philippines', 'Japan', 'Singapore', 'Thailand', 'Vietnam');

delete from per_diem where region = 'AAK' and country = 'China';






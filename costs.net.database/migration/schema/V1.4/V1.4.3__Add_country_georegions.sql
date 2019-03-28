
/* Europe */
update country
set geo_region_id = (select id from geo_region g where g."key" = 'Europe')
where geo_region_id is null and "name" in ('Albania', 'Andorra', 'Austria', 
	'Belgium', 'Bosnia and Herzegovina', 'Bulgaria',
	'Croatia', 'Cyprus', 'Czech Republic', 
	'Denmark', 'Finland', 'Germany', 'Greece', 'Guernsey',
	'Hungary', 'Iceland', 'Ireland', 'Isle of Man', 'Jersey',
	'Luxembourg', 'Monaco', 'Netherlands', 'Norway', 'Poland', 'Romania', 
	'Serbia', 'Slovakia', 'Slovenia', 'Spain', 'Switzerland', 'Sweden');
	
/* North America */
update country
set geo_region_id = (select id from geo_region g where g."key" = 'North America')
where geo_region_id is null and "name" in ('Canada');
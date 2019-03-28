--ADC-2704
update 
	country 
set 
	budget_region_id = (select id from region where "name"='IMEA')
where 
	iso in ('LS', 'BJ');
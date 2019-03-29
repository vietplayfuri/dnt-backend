do $$
	declare userName text;
begin
	if (exists (select 1 from pg_roles WHERE rolname = 'etl_reporting')) then
		raise notice 'Granting ''select'' permission to user % on all tables (current/future)', 'etl_reporting';
		
		grant select on all tables in schema public TO etl_reporting;

		alter default privileges in schema public
			grant select on tables to etl_reporting;
	end if;	
end 
$$ language plpgsql;

drop function getCostsByStageDetailsFieldValue(jsonPath text[], jsonValues text[]);

create function getCostsByStageDetailsFieldValue(jsonPath text[], jsonValues text[])
returns table(
	id uuid,
	cost_template_version_id uuid,
	created_by_id uuid,
	user_groups varchar[],
	created timestamp,
	modified timestamp,
	status int2,
	cost_type int2,
	cost_number text,
	latest_cost_stage_revision_id uuid,
	"version" int8,
	parent_id uuid,
	deleted bool,
	exchange_rate_date timestamp,
	is_external_purchases bool,
	project_id uuid,
	user_modified timestamp,
	owner_id uuid
	)
as
$$
begin
	return query
	select 
		c.*
	from 
		cost c
	join cost_stage_revision csr on csr.id = c.latest_cost_stage_revision_id
	join custom_form_data cfd on cfd.id = csr.stage_details_id
	where cfd.data::jsonb#>>jsonPath = any (jsonValues);		
end;
$$ LANGUAGE plpgsql;
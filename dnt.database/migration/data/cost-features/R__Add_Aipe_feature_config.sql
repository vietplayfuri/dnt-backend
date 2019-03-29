/* For now, this config used to show/hide UIs mentioned in ADC-2597 */
do $$
begin
	insert into feature ("name", "enabled", created_by_id, created, modified)
				  select 'Aipe', false, '77681eb0-fc0d-44cf-83a0-36d51851e9ae', now(), now()
	where not exists (select "name" from feature where "name" = 'Aipe');
end;
$$
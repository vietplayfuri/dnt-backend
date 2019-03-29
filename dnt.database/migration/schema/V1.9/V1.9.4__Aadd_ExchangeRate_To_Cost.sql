alter table cost 
	add column exchange_rate numeric null,
	add column payment_currency_id uuid null,
	add constraint cost_currency_fk foreign key (payment_currency_id) references currency(id);

CREATE INDEX cost_payment_currency_idx ON cost(payment_currency_id);

update cost set payment_currency_id = s.payment_currency_id
from
(
select
	c.id,
	c.cost_number,
	case when pd."data"::jsonb->>'directPaymentVendor' is not null then True else false end as is_dpv,
	case when pd."data"::jsonb->>'directPaymentVendor' is not null then (pd."data"::jsonb#>>'{directPaymentVendor,currencyId}')::uuid else cur.id end as payment_currency_id,
	cur.id as agency_currency_id
from cost c
	join cost_stage_revision csr on c.latest_cost_stage_revision_id = csr.id
	join custom_form_data sd on csr.stage_details_id = sd.id
	join custom_form_data pd on csr.product_details_id = pd.id
	left join currency cur on sd."data"::jsonb->>'agencyCurrency' = cur.code
)as s
where cost.id = s.id;

update cost set exchange_rate = agg.rate 
from
(
	select
		s.id,
		er.rate
	from 
	(
		select
			cc.id,
			cc.payment_currency_id,
			max(err.effective_from) as from_date 
		from
			cost cc
			join exchange_rate err on err.from_currency = cc.payment_currency_id and err.effective_from <= cc.exchange_rate_date
		where cc.exchange_rate_date is not null
		group by cc.id, cc.payment_currency_id
	) as s
	join exchange_rate er on s.payment_currency_id = er.from_currency and er.effective_from = s.from_date
) as agg
where cost.id = agg.id;

drop function getcostsbystagedetailsfieldvalue(jsonpath text[], jsonvalues text[]);

create or replace function getCostsByStageDetailsFieldValue(jsonPath text[], jsonValues text[])
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
	owner_id uuid,
	exchange_rate numeric,
	payment_currency_id uuid
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

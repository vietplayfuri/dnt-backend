ALTER TABLE public.cost_user 
	ADD notification_budget_region_id uuid NULL DEFAULT null,
	add constraint n_b_r_fk foreign key (notification_budget_region_id) references region(id);


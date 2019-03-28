
-- This is used to indicate which cost stage revision a policy exception was created in.
ALTER TABLE public.policy_exception
	add created_in_revision_id uuid null default null;

-- migrate data across even though it is not 100% accurate.
update policy_exception set created_in_revision_id = cost_stage_revision_id, modified = now();

-- set created_in_revision_id to be non null
ALTER TABLE public.policy_exception
	ALTER COLUMN created_in_revision_id SET NOT null;
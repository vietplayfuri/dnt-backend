
CREATE TABLE public.approval_details (
	id uuid NOT NULL DEFAULT uuid_generate_v1(),
	approver_id uuid NOT NULL,
	created_by_id uuid NOT NULL,
	created TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT (now() AT TIME ZONE 'utc')
);
CREATE INDEX approval_details_idx ON approval_details (approver_id);

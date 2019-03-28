CREATE TABLE public.cost_owner(
  id uuid NOT NULL DEFAULT uuid_generate_v1(),
  cost_id uuid NOT NULL,
  user_id uuid NOT NULL,
  end_date timestamp NOT NULL DEFAULT timezone('utc'::text, now()),
  PRIMARY KEY (id),
  FOREIGN KEY (cost_id) REFERENCES cost(id),
  FOREIGN KEY (user_id) REFERENCES cost_user(id)
)
WITH (
    OIDS=FALSE
);
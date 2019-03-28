alter table cost 
    add owner_id uuid,
    add constraint cost_owner_fk foreign key (owner_id) references cost_user(id);

update cost set
    owner_id = created_by_id;

alter table cost
    alter column owner_id set not null;
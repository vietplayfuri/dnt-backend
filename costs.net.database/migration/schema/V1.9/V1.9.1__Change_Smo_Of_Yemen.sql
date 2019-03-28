update country set
    smo_id = (select id from smo where key = 'GDM IMEA ALL OTHER CLUSTER')
where name = 'Yemen';
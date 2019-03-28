delete from approval_member
where id in (
select id from (
select ROW_NUMBER () OVER (ORDER BY approval_id) row_number, *
from approval_member
where approval_id in (
select approval_id from (
select approval_id, member_id from approval_member
group by approval_id, member_id
having count(member_id) > 1) as d1)) as d2
where row_number % 2 = 0);


ALTER TABLE approval_member DROP CONSTRAINT IF EXISTS am_uq_appid_memberid;
ALTER TABLE approval_member ADD CONSTRAINT am_uq_appid_memberid UNIQUE (approval_id, member_id);
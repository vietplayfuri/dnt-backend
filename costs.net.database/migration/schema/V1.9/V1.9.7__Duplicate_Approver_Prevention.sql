--find revisions with duplicated approvals
SELECT cost_stage_revision_id AS csr_id, "type"
  INTO TEMP_approval_affected_revisions
  FROM approval
  GROUP BY cost_stage_revision_id, "type"
  HAVING count(*) > 1;

--find last-modified approval of affected revisions
---this table is here because postgres is not allowing ORDER BY in subqueries
SELECT DISTINCT ON (cost_stage_revision_id)
    id, cost_stage_revision_id
  INTO TEMP_approval_ids_to_preserve
  FROM approval app
  INNER JOIN TEMP_approval_affected_revisions tmp ON app.cost_stage_revision_id = tmp.csr_id AND app."type" = tmp."type"
  ORDER BY cost_stage_revision_id, modified DESC;

--find approvals taht should be deleted
SELECT app.id
  INTO TEMP_approval_ids_to_delete
  FROM approval app
  INNER JOIN TEMP_approval_affected_revisions tmp ON app.cost_stage_revision_id = tmp.csr_id AND app."type" = tmp."type"
  WHERE app.id NOT IN (SELECT id FROM TEMP_approval_ids_to_preserve);

--delete members
DELETE FROM approval_member
  WHERE status = 0
    AND approval_id IN (SELECT id FROM TEMP_approval_ids_to_delete);

--delete approvals
DELETE FROM approval
  WHERE status = 0
    AND id IN (SELECT id FROM TEMP_approval_ids_to_delete);

--delete temporary tables
DROP TABLE TEMP_approval_affected_revisions;
DROP TABLE TEMP_approval_ids_to_preserve;
DROP TABLE TEMP_approval_ids_to_delete;

--create unique so it would never happen again
ALTER TABLE approval
  ADD CONSTRAINT approval_uq_csrid_type UNIQUE (cost_stage_revision_id, "type");
UPDATE public.cost_stage
SET name='Current Revision'
WHERE "name" in ('Original Estimate Revision', 'Final Estimate Revision');
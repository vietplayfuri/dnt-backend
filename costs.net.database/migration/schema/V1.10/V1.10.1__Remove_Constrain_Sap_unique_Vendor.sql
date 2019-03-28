/* ADC-2715 - because SAP is unique key, so even the row has deleted = true, user still can't re-use this SAP vendor */
ALTER TABLE vendor DROP CONSTRAINT IF EXISTS vendor_un;
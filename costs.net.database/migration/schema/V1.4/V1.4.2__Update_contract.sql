UPDATE dictionary_entry
  SET value = 'Contract (celebrity & athletes)'
  WHERE dictionary_id = (SELECT d.id FROM dictionary d WHERE d."name" = 'UsageBuyoutType' )
    AND "key" = 'Contract';
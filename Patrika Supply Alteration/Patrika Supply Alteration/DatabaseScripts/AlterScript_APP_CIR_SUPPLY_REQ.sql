-- ============================================================
-- ALTER SCRIPT: APP_CIR_SUPPLY_REQ
-- Run this on live database if the table already exists
-- and you need to add the newer columns.
-- ============================================================
-- Skip any ALTER that already exists on your live table.
-- ============================================================

-- Day-wise supply columns (added for day-wise supply feature)
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_MON NUMBER;
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_TUE NUMBER;
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_WED NUMBER;
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_THU NUMBER;
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_FRI NUMBER;
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_SAT NUMBER;
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_SUN NUMBER;
ALTER TABLE APP_CIR_SUPPLY_REQ ADD IS_DAYWISE_SUPPLY NUMBER DEFAULT 0;

-- ERP push date column (if not already present)
ALTER TABLE APP_CIR_SUPPLY_REQ ADD ERP_PUSH_DATE DATE;

-- UNIT_CODE column (branch/unit tracking)
ALTER TABLE APP_CIR_SUPPLY_REQ ADD UNIT_CODE VARCHAR2(20);

-- SUPPLY_TYPE_CODE column
ALTER TABLE APP_CIR_SUPPLY_REQ ADD SUPPLY_TYPE_CODE VARCHAR2(10);

-- CHANGED_SUPPLY_DATE column
ALTER TABLE APP_CIR_SUPPLY_REQ ADD CHANGED_SUPPLY_DATE DATE;

-- REMARKS column
ALTER TABLE APP_CIR_SUPPLY_REQ ADD REMARKS VARCHAR2(500);

-- ============================================================
-- END OF ALTER SCRIPT
-- ============================================================

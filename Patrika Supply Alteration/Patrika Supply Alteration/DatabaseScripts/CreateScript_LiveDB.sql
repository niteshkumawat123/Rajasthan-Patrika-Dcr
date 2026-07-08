-- ============================================================
-- DCR Supply Alteration App - Database Create Script for Live DB
-- Oracle Database
-- ============================================================
-- 
-- SUMMARY:
-- --------------------------------------------------------
-- NEW TABLES CREATED:          3
--   1. APP_CIR_SUPPLY_REQ          (Supply alteration requests)
--   2. APP_CIR_SUPPLY_APPROVAL     (Approval workflow tracking)
--   3. APP_CIR_HO_APPROVAL_MAST   (HO user-branch mapping)
--
-- NEW SEQUENCES CREATED:       2
--   1. SEQ_SUPPLY_REQ
--   2. SEQ_SUPPLY_APPROVAL
--
-- EXISTING TABLES MODIFIED:    1
--   1. LOGIN (3 new columns added)
--      - SUPPLY_ALTERATION_PASSWORD
--      - FIRST_LOGIN_FLAG
--      - PUSH_TOKEN
--
-- EXISTING TABLES USED (READ/UPDATE, no structural change):
--   - CIR_PLI_HIERARCHY
--   - CIR_PLI_HIERARCHY_MAST
--   - HR_EMP_MST
--   - CIR_EXECUTIVE_MAST
--   - PUB_CENT_MAST
--   - CIR_AGMAST
--   - CIR_SUPPLY (updated BASE_SUPPLY, day-wise columns, SUPPLY_EFFECTIVE_DATE, UPDATED_BY, UPDATED_DT)
--   - CIR_DROP_POINT_MAST
-- ============================================================


-- ============================================================
-- STEP 1: CREATE SEQUENCES
-- ============================================================

-- Sequence for APP_CIR_SUPPLY_REQ.REQ_ID
CREATE SEQUENCE SEQ_SUPPLY_REQ
    START WITH 1
    INCREMENT BY 1
    NOCACHE
    NOCYCLE;

-- Sequence for APP_CIR_SUPPLY_APPROVAL.APPROVAL_ID
CREATE SEQUENCE SEQ_SUPPLY_APPROVAL
    START WITH 1
    INCREMENT BY 1
    NOCACHE
    NOCYCLE;


-- ============================================================
-- STEP 2: CREATE NEW TABLES
-- ============================================================

-- Table 1: APP_CIR_SUPPLY_REQ (Supply Alteration Requests)
CREATE TABLE APP_CIR_SUPPLY_REQ (
    REQ_ID              NUMBER          NOT NULL,
    COMP_CODE           VARCHAR2(10)    NOT NULL,
    UNIT_CODE           VARCHAR2(20),
    AGCD                VARCHAR2(20)    NOT NULL,
    DPCD                VARCHAR2(20)    NOT NULL,
    PUBL                VARCHAR2(10),
    EDTN                VARCHAR2(10),
    SUPPLY_TYPE_CODE    VARCHAR2(10),
    BASE_SUPPLY         NUMBER,
    INC_DEC             VARCHAR2(5),        -- 'I' = Increase, 'D' = Decrease
    CHANGED_SUPPLY      NUMBER,
    REASON_CODE         VARCHAR2(50),
    ZONE_CODE           VARCHAR2(20),
    USERID              VARCHAR2(50),       -- Employee code who submitted
    CREATION_DATE       DATE,
    CHANGED_SUPPLY_DATE DATE,
    REMARKS             VARCHAR2(500),
    STATUS              VARCHAR2(30),       -- PENDING_ZH, PENDING_HO, HO_APPROVED, ZH_REJECTED, HO_REJECTED
    ERP_PUSH_FLAG       VARCHAR2(5),        -- 'Y' or 'N'
    ERP_PUSH_DATE       DATE,
    SUPPLY_MON          NUMBER,
    SUPPLY_TUE          NUMBER,
    SUPPLY_WED          NUMBER,
    SUPPLY_THU          NUMBER,
    SUPPLY_FRI          NUMBER,
    SUPPLY_SAT          NUMBER,
    SUPPLY_SUN          NUMBER,
    IS_DAYWISE_SUPPLY   NUMBER DEFAULT 0,   -- 0 = No, 1 = Yes
    CONSTRAINT PK_APP_CIR_SUPPLY_REQ PRIMARY KEY (REQ_ID)
);

-- Indexes for APP_CIR_SUPPLY_REQ
CREATE INDEX IDX_SUPPLY_REQ_USERID ON APP_CIR_SUPPLY_REQ (USERID);
CREATE INDEX IDX_SUPPLY_REQ_STATUS ON APP_CIR_SUPPLY_REQ (STATUS);
CREATE INDEX IDX_SUPPLY_REQ_COMP ON APP_CIR_SUPPLY_REQ (COMP_CODE);
CREATE INDEX IDX_SUPPLY_REQ_UNIT ON APP_CIR_SUPPLY_REQ (UNIT_CODE);
CREATE INDEX IDX_SUPPLY_REQ_AGCD ON APP_CIR_SUPPLY_REQ (AGCD, DPCD);
CREATE INDEX IDX_SUPPLY_REQ_CREATION ON APP_CIR_SUPPLY_REQ (CREATION_DATE);
CREATE INDEX IDX_SUPPLY_REQ_ERP ON APP_CIR_SUPPLY_REQ (ERP_PUSH_FLAG, ERP_PUSH_DATE);


-- Table 2: APP_CIR_SUPPLY_APPROVAL (Approval Workflow)
CREATE TABLE APP_CIR_SUPPLY_APPROVAL (
    APPROVAL_ID         NUMBER          NOT NULL,
    REQ_ID              NUMBER          NOT NULL,
    COMP_CODE           VARCHAR2(10),
    -- ZH (Zonal Head) Approval Fields
    ZH_ACTION           VARCHAR2(20),       -- 'APPROVED' or 'REJECTED'
    ZH_ACTION_BY        VARCHAR2(50),       -- Employee code of ZH
    ZH_ACTION_DATE      DATE,
    ZH_REMARKS          VARCHAR2(500),
    ZH_FROM_STATUS      VARCHAR2(30),
    ZH_TO_STATUS        VARCHAR2(30),
    -- HO (Head Office) Approval Fields
    HO_ACTION           VARCHAR2(20),       -- 'APPROVED' or 'REJECTED'
    HO_ACTION_BY        VARCHAR2(50),       -- Employee code of HO user
    HO_ACTION_DATE      DATE,
    HO_REMARKS          VARCHAR2(500),
    HO_FROM_STATUS      VARCHAR2(30),
    HO_TO_STATUS        VARCHAR2(30),
    -- ERP Push Tracking
    ERP_PUSHED_BY       VARCHAR2(50),
    ERP_PUSHED_DATE     DATE,
    -- Overall Status
    STATUS              VARCHAR2(30),       -- Mirrors the final status
    CONSTRAINT PK_APP_CIR_SUPPLY_APPROVAL PRIMARY KEY (APPROVAL_ID)
);

-- Indexes for APP_CIR_SUPPLY_APPROVAL
CREATE INDEX IDX_SUPPLY_APPR_REQID ON APP_CIR_SUPPLY_APPROVAL (REQ_ID);
CREATE INDEX IDX_SUPPLY_APPR_ZH ON APP_CIR_SUPPLY_APPROVAL (ZH_ACTION_BY);
CREATE INDEX IDX_SUPPLY_APPR_HO ON APP_CIR_SUPPLY_APPROVAL (HO_ACTION_BY);
CREATE INDEX IDX_SUPPLY_APPR_ERP ON APP_CIR_SUPPLY_APPROVAL (ERP_PUSHED_BY);


-- Table 3: APP_CIR_HO_APPROVAL_MAST (HO User Branch Mapping)
CREATE TABLE APP_CIR_HO_APPROVAL_MAST (
    EMPLOYEE_CODE       VARCHAR2(50)    NOT NULL,
    BRANCH_CODE         VARCHAR2(20)    NOT NULL,
    IS_ACTIVE           VARCHAR2(5)     DEFAULT 'Y',   -- 'Y' or 'N'
    EMAIL_ID            VARCHAR2(100),
    CONSTRAINT PK_HO_APPROVAL_MAST PRIMARY KEY (EMPLOYEE_CODE, BRANCH_CODE)
);

-- Index for APP_CIR_HO_APPROVAL_MAST
CREATE INDEX IDX_HO_APPR_ACTIVE ON APP_CIR_HO_APPROVAL_MAST (IS_ACTIVE);


-- ============================================================
-- STEP 3: ALTER EXISTING TABLE - LOGIN
-- (Add 3 new columns for the Supply Alteration App)
-- ============================================================

-- Column 1: Password for Supply Alteration App login
ALTER TABLE LOGIN ADD SUPPLY_ALTERATION_PASSWORD VARCHAR2(100);

-- Column 2: First login flag (0 = first time, 1 = password changed)
ALTER TABLE LOGIN ADD FIRST_LOGIN_FLAG NUMBER DEFAULT 0;

-- Column 3: Push notification token for Firebase
ALTER TABLE LOGIN ADD PUSH_TOKEN VARCHAR2(500);


-- ============================================================
-- STEP 4: VERIFY CIR_SUPPLY TABLE HAS REQUIRED COLUMNS
-- (These columns are UPDATED by the app. If missing, add them)
-- ============================================================

-- Uncomment below if UPDATED_BY and UPDATED_DT columns don't exist in CIR_SUPPLY:
-- ALTER TABLE CIR_SUPPLY ADD UPDATED_BY VARCHAR2(50);
-- ALTER TABLE CIR_SUPPLY ADD UPDATED_DT DATE;


-- ============================================================
-- STEP 5: GRANT PERMISSIONS (if needed)
-- Replace <APP_USER> with your actual application database user
-- ============================================================

-- GRANT SELECT, INSERT, UPDATE ON APP_CIR_SUPPLY_REQ TO <APP_USER>;
-- GRANT SELECT, INSERT, UPDATE ON APP_CIR_SUPPLY_APPROVAL TO <APP_USER>;
-- GRANT SELECT ON APP_CIR_HO_APPROVAL_MAST TO <APP_USER>;
-- GRANT SELECT ON SEQ_SUPPLY_REQ TO <APP_USER>;
-- GRANT SELECT ON SEQ_SUPPLY_APPROVAL TO <APP_USER>;


-- ============================================================
-- END OF SCRIPT
-- ============================================================

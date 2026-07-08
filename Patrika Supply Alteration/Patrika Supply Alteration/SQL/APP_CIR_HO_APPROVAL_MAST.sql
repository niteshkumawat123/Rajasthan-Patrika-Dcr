-- =====================================================
-- Table: APP_CIR_HO_APPROVAL_MAST
-- Purpose: Defines which HO users can approve entries 
--          for which branches. A single user can have
--          multiple branch entries.
-- =====================================================

CREATE TABLE APP_CIR_HO_APPROVAL_MAST (
    ID              NUMBER         NOT NULL,
    EMPLOYEE_CODE   VARCHAR2(20)   NOT NULL,
    BRANCH_CODE     VARCHAR2(20)   NOT NULL,
    EMAIL_ID        VARCHAR2(100),
    IS_ACTIVE       CHAR(1)        DEFAULT 'Y' NOT NULL,
    CREATED_BY      VARCHAR2(20),
    CREATED_DATE    DATE           DEFAULT SYSDATE,
    UPDATED_BY      VARCHAR2(20),
    UPDATED_DATE    DATE,
    CONSTRAINT PK_HO_APPROVAL_MAST PRIMARY KEY (ID),
    CONSTRAINT CHK_HO_MAST_ACTIVE CHECK (IS_ACTIVE IN ('Y', 'N'))
);

-- Sequence for ID generation
CREATE SEQUENCE SEQ_HO_APPROVAL_MAST START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;

-- Trigger to auto-populate ID from sequence
CREATE OR REPLACE TRIGGER TRG_HO_APPROVAL_MAST_ID
BEFORE INSERT ON APP_CIR_HO_APPROVAL_MAST
FOR EACH ROW
BEGIN
    IF :NEW.ID IS NULL THEN
        SELECT SEQ_HO_APPROVAL_MAST.NEXTVAL INTO :NEW.ID FROM DUAL;
    END IF;
END;
/

-- Index for fast lookup by employee code
CREATE INDEX IDX_HO_APPROVAL_EMP ON APP_CIR_HO_APPROVAL_MAST (EMPLOYEE_CODE, IS_ACTIVE);

-- Index for branch code lookup
CREATE INDEX IDX_HO_APPROVAL_BRANCH ON APP_CIR_HO_APPROVAL_MAST (BRANCH_CODE, IS_ACTIVE);

-- Unique constraint: one employee cannot have duplicate branch entry
CREATE UNIQUE INDEX IDX_HO_APPROVAL_UNIQUE ON APP_CIR_HO_APPROVAL_MAST (EMPLOYEE_CODE, BRANCH_CODE);

-- =====================================================
-- Sample Insert (for reference):
-- INSERT INTO APP_CIR_HO_APPROVAL_MAST (ID, EMPLOYEE_CODE, BRANCH_CODE, EMAIL_ID, IS_ACTIVE, CREATED_BY)
-- VALUES (SEQ_HO_APPROVAL_MAST.NEXTVAL, 'EMP001', 'BR001', 'user@example.com', 'Y', 'ADMIN');
--
-- INSERT INTO APP_CIR_HO_APPROVAL_MAST (ID, EMPLOYEE_CODE, BRANCH_CODE, EMAIL_ID, IS_ACTIVE, CREATED_BY)
-- VALUES (SEQ_HO_APPROVAL_MAST.NEXTVAL, 'EMP001', 'BR002', 'user@example.com', 'Y', 'ADMIN');
-- =====================================================

COMMIT;

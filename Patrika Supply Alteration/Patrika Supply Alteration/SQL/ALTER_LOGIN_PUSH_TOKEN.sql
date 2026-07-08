-- Add PUSH_TOKEN column to LOGIN table to store FCM device tokens
ALTER TABLE LOGIN ADD (PUSH_TOKEN VARCHAR2(500));

-- Index for faster token lookups
CREATE INDEX IDX_LOGIN_PUSH_TOKEN ON LOGIN (HR_CODE, PUSH_TOKEN);

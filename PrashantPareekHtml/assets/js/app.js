
// ════════════════════════════════════════════
//  MOCK ERP DATA
// ════════════════════════════════════════════
const ERP_AGENTS = {
  'JP-1021': { name: 'Mohan Lal Gupta',    zone: 'Jaipur North',  branch:'Branch A', supply: 45, se: 'SE-001' },
  'JP-1045': { name: 'Sunita Devi',         zone: 'Vaishali Nagar', branch:'Branch B', supply: 30, se: 'SE-001' },
  'JP-1063': { name: 'Ramesh Chandra',      zone: 'Malviya Nagar', branch:'Branch C', supply: 70, se: 'SE-002' },
  'JP-1072': { name: 'Priya Sharma',        zone: 'Sanganer',       branch:'Branch D', supply: 25, se: 'SE-001' },
  'JP-1088': { name: 'Deepak Verma',        zone: 'Jaipur East',   branch:'Branch E', supply: 55, se: 'SE-002' },
  'JP-1099': { name: 'Kavita Joshi',        zone: 'Jaipur South',  branch:'Branch F', supply: 40, se: 'SE-003' },
  'JP-1110': { name: 'Suresh Meena',        zone: 'Mansarovar',    branch:'Branch G', supply: 80, se: 'SE-001' },
  'JP-1125': { name: 'Anita Kumari',        zone: 'Jaipur West',   branch:'Branch H', supply: 35, se: 'SE-003' },
  'JP-1140': { name: 'Bharat Lal',          zone: 'Jaipur North',  branch:'Branch A', supply: 50, se: 'SE-002' },
  'JP-1155': { name: 'Geeta Rawat',         zone: 'Sanganer',      branch:'Branch D', supply: 22, se: 'SE-003' },
};

const ERP_USERS = {
  'se': [
    { id:'SE-001', name:'Rajesh Sharma',  mobile:'9876543210', zone:'Jaipur North', zoneHead:'ZH-001', agents:['JP-1021','JP-1045','JP-1072','JP-1110'] },
    { id:'SE-002', name:'Meena Gupta',    mobile:'9988776655', zone:'Jaipur East',  zoneHead:'ZH-001', agents:['JP-1063','JP-1088','JP-1140'] },
    { id:'SE-003', name:'Arjun Yadav',   mobile:'9911223344', zone:'Jaipur South', zoneHead:'ZH-002', agents:['JP-1099','JP-1125','JP-1155'] },
  ],
  'zh': [
    { id:'ZH-001', name:'Vikram Singh',   mobile:'9800011122', zone:'North & East Jaipur', reportTo:'HO-001', execCount:2 },
    { id:'ZH-002', name:'Pooja Saxena',   mobile:'9700022233', zone:'South & West Jaipur', reportTo:'HO-001', execCount:1 },
  ],
  'ho': [
    { id:'HO-001', name:'Anand Mathur',   mobile:'9600033344', office:'Jaipur Head Office' },
  ]
};

// ════════════════════════════════════════════
//  SHARED REQUEST DATA
// ════════════════════════════════════════════
const REQUESTS = [
  { id:'REQ001', agentCode:'JP-1021', agent:'Mohan Lal Gupta', zone:'Jaipur North', branch:'Branch A', seId:'SE-001', seName:'Rajesh Sharma', zhId:'ZH-001',
    type:'increase', oldSupply:45, newSupply:60, reason:'New residential colony opened', remarks:'Sector 12 colony added ~200 homes', effDate:'2026-05-10',
    status:'erp_done', pipelineStep:5, submittedAt:'09:30 AM', zhRemarks:'Verified. Colony confirmed.', hoRemarks:'Approved. ERP updated.' },
  { id:'REQ002', agentCode:'JP-1045', agent:'Sunita Devi', zone:'Vaishali Nagar', branch:'Branch B', seId:'SE-001', seName:'Rajesh Sharma', zhId:'ZH-001',
    type:'increase', oldSupply:30, newSupply:38, reason:'Subscriber base increased', remarks:'8 new subscribers last month', effDate:'2026-05-11',
    status:'ho_review', pipelineStep:3, submittedAt:'11:15 AM', zhRemarks:'Verified subscriber list.', hoRemarks:'' },
  { id:'REQ003', agentCode:'JP-1063', agent:'Ramesh Chandra', zone:'Malviya Nagar', branch:'Branch C', seId:'SE-002', seName:'Meena Gupta', zhId:'ZH-001',
    type:'decrease', oldSupply:70, newSupply:55, reason:'Subscriber base decreased', remarks:'15 cancelled subscriptions', effDate:'2026-05-09',
    status:'rejected', pipelineStep:2, submittedAt:'02:45 PM', zhRemarks:'', hoRemarks:'Rejected - Data mismatch. Resubmit.' },
  { id:'REQ004', agentCode:'JP-1072', agent:'Priya Sharma', zone:'Sanganer', branch:'Branch D', seId:'SE-001', seName:'Rajesh Sharma', zhId:'ZH-001',
    type:'increase', oldSupply:25, newSupply:40, reason:'Festival / seasonal demand', remarks:'Diwali week extra demand expected', effDate:'2026-05-11',
    status:'pending', pipelineStep:2, submittedAt:'10:00 AM', zhRemarks:'', hoRemarks:'' },
  { id:'REQ005', agentCode:'JP-1088', agent:'Deepak Verma', zone:'Jaipur East', branch:'Branch E', seId:'SE-002', seName:'Meena Gupta', zhId:'ZH-001',
    type:'increase', oldSupply:55, newSupply:65, reason:'Agency\'s area expanded', remarks:'Taking over JP-1100 area', effDate:'2026-05-12',
    status:'pending', pipelineStep:2, submittedAt:'01:20 PM', zhRemarks:'', hoRemarks:'' },
  { id:'REQ006', agentCode:'JP-1099', agent:'Kavita Joshi', zone:'Jaipur South', branch:'Branch F', seId:'SE-003', seName:'Arjun Yadav', zhId:'ZH-002',
    type:'decrease', oldSupply:40, newSupply:32, reason:'Competition impact', remarks:'Rival paper gaining in area', effDate:'2026-05-10',
    status:'ho_review', pipelineStep:3, submittedAt:'08:45 AM', zhRemarks:'Confirmed by field visit.', hoRemarks:'' },
  { id:'REQ007', agentCode:'JP-1110', agent:'Suresh Meena', zone:'Mansarovar', branch:'Branch G', seId:'SE-001', seName:'Rajesh Sharma', zhId:'ZH-001',
    type:'increase', oldSupply:80, newSupply:95, reason:'Subscriber base increased', remarks:'New corporate office subscriptions', effDate:'2026-05-08',
    status:'approved', pipelineStep:4, submittedAt:'09:00 AM', zhRemarks:'Verified.', hoRemarks:'Approved. ERP update pending cut-off.' },
];

// ════════════════════════════════════════════
//  PASSWORD STORE (username -> {password, isDefault, email})
// ════════════════════════════════════════════
const USER_CREDENTIALS = {
  'SE-001': { password:'password123', isDefault:true, email:'rajesh.sharma@dcr.in' },
  'SE-002': { password:'password123', isDefault:true, email:'meena.gupta@dcr.in' },
  'SE-003': { password:'password123', isDefault:true, email:'arjun.yadav@dcr.in' },
  'ZH-001': { password:'password123', isDefault:true, email:'vikram.singh@dcr.in' },
  'ZH-002': { password:'password123', isDefault:true, email:'pooja.saxena@dcr.in' },
  'HO-001': { password:'password123', isDefault:true, email:'anand.mathur@dcr.in' },
};

// ════════════════════════════════════════════
//  APP STATE
// ════════════════════════════════════════════
let currentRole = null;
let currentUser = null;
let selectedType = 'increase';
let pendingRemarksAction = null;
let activeDetailId = null;
let hoDashboardDate = today();
let hoDetailFilter = { zone: '', branch: '' };

// ════════════════════════════════════════════
//  SPLASH
// ════════════════════════════════════════════
setTimeout(() => document.getElementById('splash').classList.add('hide'), 2200);

// ════════════════════════════════════════════
//  LOGIN — ROLE SELECTION (click role → go to login form)
// ════════════════════════════════════════════
function selectRole(r) {
  currentRole = r;
  document.querySelectorAll('.role-card').forEach(c => c.classList.remove('selected','se','zh','ho'));
  document.getElementById('rc-'+r).classList.add('selected', r);
  // Immediately show username/password step
  showLoginStep();
}

function showLoginStep() {
  document.getElementById('step-role').style.display = 'none';
  document.getElementById('step-login').style.display = 'block';
  document.getElementById('login-username').value = '';
  document.getElementById('login-password').value = '';
  document.getElementById('login-error').style.display = 'none';
}

function backToRoleFromLogin() {
  document.getElementById('step-login').style.display = 'none';
  document.getElementById('step-role').style.display = 'block';
  currentRole = null;
  document.querySelectorAll('.role-card').forEach(c=>c.classList.remove('selected','se','zh','ho'));
}

function doLogin() {
  const username = document.getElementById('login-username').value.trim().toUpperCase();
  const password = document.getElementById('login-password').value;
  const errEl = document.getElementById('login-error');

  const cred = USER_CREDENTIALS[username];
  if (!cred) { errEl.textContent = 'Invalid username.'; errEl.style.display='block'; return; }
  if (cred.password !== password) { errEl.textContent = 'Incorrect password.'; errEl.style.display='block'; return; }

  // Determine which user this is
  let matchedUser = null;
  for (const roleKey of ['se','zh','ho']) {
    const found = ERP_USERS[roleKey].find(u => u.id === username);
    if (found) { matchedUser = { ...found, role: roleKey }; break; }
  }
  if (!matchedUser) { errEl.textContent = 'User not found.'; errEl.style.display='block'; return; }

  // Role mismatch check
  if (matchedUser.role !== currentRole) {
    errEl.textContent = 'This ID does not belong to the selected role.';
    errEl.style.display='block';
    return;
  }

  currentUser = matchedUser;
  errEl.style.display = 'none';

  if (cred.isDefault) {
    // Force change password
    document.getElementById('step-login').style.display = 'none';
    document.getElementById('step-change-pw').style.display = 'block';
    document.getElementById('cp-new').value='';
    document.getElementById('cp-confirm').value='';
    document.getElementById('cp-error').style.display='none';
  } else {
    loginSuccess();
  }
}

function doChangePassword() {
  const np = document.getElementById('cp-new').value;
  const cp = document.getElementById('cp-confirm').value;
  const errEl = document.getElementById('cp-error');

  if (np.length < 6) { errEl.textContent='Password must be at least 6 characters.'; errEl.style.display='block'; return; }
  if (np === 'password123') { errEl.textContent='New password cannot be the default password.'; errEl.style.display='block'; return; }
  if (np !== cp) { errEl.textContent='Passwords do not match.'; errEl.style.display='block'; return; }

  USER_CREDENTIALS[currentUser.id].password = np;
  USER_CREDENTIALS[currentUser.id].isDefault = false;
  document.getElementById('step-change-pw').style.display='none';
  showToast('✅ Password changed successfully!');
  loginSuccess();
}

function doForgotPassword() {
  const username = document.getElementById('login-username').value.trim().toUpperCase();
  const errEl = document.getElementById('login-error');
  if (!username) { errEl.textContent='Enter your Employee ID first.'; errEl.style.display='block'; return; }
  const cred = USER_CREDENTIALS[username];
  if (!cred) { errEl.textContent='Employee ID not found.'; errEl.style.display='block'; return; }
  errEl.style.display='none';
  showToast(`✅ Current password sent to ${cred.email}`);
}

// ════════════════════════════════════════════
//  LOGIN SUCCESS
// ════════════════════════════════════════════
function loginSuccess() {
  if (currentRole === 'se') {
    document.getElementById('se-name-disp').textContent = currentUser.name;
    document.getElementById('se-profile-name').textContent = currentUser.name;
    document.getElementById('se-emp-id').textContent = currentUser.id;
    document.getElementById('se-zone-prof').textContent = currentUser.zone;
    const zh = ERP_USERS.zh.find(z=>z.id===currentUser.zoneHead);
    document.getElementById('se-reporting').textContent = zh ? zh.name : 'Zonal Head';
    renderSEDashboard();
    setScreen('se-dashboard');
  } else if (currentRole === 'zh') {
    document.getElementById('zh-name-disp').textContent = currentUser.name;
    document.getElementById('zh-profile-name').textContent = currentUser.name;
    document.getElementById('zh-emp-id').textContent = currentUser.id;
    document.getElementById('zh-zone-prof').textContent = currentUser.zone;
    document.getElementById('zh-exec-count').textContent = currentUser.execCount+' Executives';
    renderZHDashboard();
    setScreen('zh-dashboard');
  } else {
    document.getElementById('ho-name-disp').textContent = currentUser.name;
    document.getElementById('ho-profile-name').textContent = currentUser.name;
    document.getElementById('ho-emp-id').textContent = currentUser.id;
    hoDashboardDate = today();
    renderHODashboard();
    setScreen('ho-dashboard');
  }
  showToast('✅ Welcome, '+currentUser.name.split(' ')[0]+'!');
}

function logout() {
  currentRole = null; currentUser = null;
  document.getElementById('step-role').style.display = 'block';
  document.getElementById('step-login').style.display = 'none';
  document.getElementById('step-change-pw').style.display = 'none';
  document.querySelectorAll('.role-card').forEach(c=>c.classList.remove('selected','se','zh','ho'));
  setScreen('login');
  showToast('Logged out.');
}

// ════════════════════════════════════════════
//  SCREEN ROUTING
// ════════════════════════════════════════════
function setScreen(id) {
  document.querySelectorAll('.screen').forEach(s=>s.classList.remove('active'));
  document.getElementById('screen-'+id).classList.add('active');
}
function seNav(to) { setScreen('se-'+to); if(to==='dashboard') renderSEDashboard(); if(to==='history') renderSEHistory(); }
function zhNav(to) { setScreen('zh-'+to); if(to==='dashboard') renderZHDashboard(); if(to==='history') renderZHHistory(); }
function hoNav(to) {
  setScreen('ho-'+to);
  if(to==='dashboard') renderHODashboard();
  if(to==='history') renderHOHistory();
  if(to==='reports') renderHOReports();
  if(to==='detail') {}
}

function seBottomNav(id, btn) {
  document.querySelectorAll('#screen-se-dashboard .nav-btn, #screen-se-new-request .nav-btn, #screen-se-history .nav-btn, #screen-se-profile .nav-btn').forEach(b=>b.classList.remove('active'));
  btn.classList.add('active');
  setScreen(id);
  if(id==='se-history') renderSEHistory();
  if(id==='se-dashboard') renderSEDashboard();
  if(id==='se-new-request') initNewRequestForm();
}
function zhBottomNav(id, btn) {
  document.querySelectorAll('#screen-zh-dashboard .nav-btn, #screen-zh-history .nav-btn, #screen-zh-profile .nav-btn').forEach(b=>b.classList.remove('active'));
  btn.classList.add('active');
  setScreen(id);
  if(id==='zh-history') renderZHHistory();
  if(id==='zh-dashboard') renderZHDashboard();
}
function hoBottomNav(id, btn) {
  document.querySelectorAll('#screen-ho-dashboard .nav-btn,#screen-ho-history .nav-btn,#screen-ho-reports .nav-btn,#screen-ho-profile .nav-btn').forEach(b=>b.classList.remove('active'));
  btn.classList.add('active');
  setScreen(id);
  if(id==='ho-history') renderHOHistory();
  if(id==='ho-dashboard') renderHODashboard();
  if(id==='ho-reports') renderHOReports();
}

// ════════════════════════════════════════════
//  SE DASHBOARD
// ════════════════════════════════════════════
function renderSEDashboard() {
  const myReqs = REQUESTS.filter(r=>r.seId===currentUser.id);
  document.getElementById('se-stat-pending').textContent  = myReqs.filter(r=>r.status==='pending').length;
  document.getElementById('se-stat-approved').textContent = myReqs.filter(r=>['approved','erp_done'].includes(r.status)).length;
  document.getElementById('se-stat-today').textContent    = myReqs.filter(r=>r.effDate===today()).length;
  document.getElementById('se-stat-rejected').textContent = myReqs.filter(r=>r.status==='rejected').length;
  renderCutoffBanner();
  const recent = myReqs.slice(0,4);
  const el = document.getElementById('se-recent-list');
  el.innerHTML = recent.length ? recent.map(r=>reqCard(r,'se')).join('') : emptyState('No requests yet.');
}

function renderCutoffBanner() {
  const now = new Date(), h=now.getHours(), m=now.getMinutes();
  const el = document.getElementById('se-cutoff-text');
  if (h < 17) {
    const rem = (17*60)-(h*60+m);
    el.innerHTML = `<b>✅ Before Cut-off (5:00 PM)</b> — Requests submitted now will process <b>today</b>. ${Math.floor(rem/60)}h ${rem%60}m remaining.`;
  } else {
    el.innerHTML = `<b>⚠️ After Cut-off (5:00 PM)</b> — Requests submitted now will process <b>next day</b>.`;
  }
}

function today() { return new Date().toISOString().split('T')[0]; }

function renderSEHistory(filter='') {
  const myReqs = REQUESTS.filter(r=>r.seId===currentUser.id &&
    (r.agent.toLowerCase().includes(filter.toLowerCase()) || r.zone.toLowerCase().includes(filter.toLowerCase())));
  const el = document.getElementById('se-history-list');
  el.innerHTML = myReqs.length ? myReqs.map(r=>reqCard(r,'se')).join('') : emptyState('No requests found.');
}
function seFilterHistory(v) { renderSEHistory(v); }

// ════════════════════════════════════════════
//  ZH DASHBOARD
// ════════════════════════════════════════════
function renderZHDashboard() {
  const zhReqs = REQUESTS.filter(r=>r.zhId===currentUser.id);
  const myPending = zhReqs.filter(r=>r.status==='pending');
  const myApproved = zhReqs.filter(r=>['ho_review','approved','erp_done'].includes(r.status));
  const atHO = zhReqs.filter(r=>r.status==='ho_review');
  const rejected = zhReqs.filter(r=>r.status==='rejected');
  document.getElementById('zh-stat-pending').textContent  = myPending.length;
  document.getElementById('zh-stat-approved').textContent = myApproved.length;
  document.getElementById('zh-stat-ho').textContent       = atHO.length;
  document.getElementById('zh-stat-rejected').textContent = rejected.length;
  document.getElementById('zh-pending-count-badge').textContent = myPending.length;

  const el = document.getElementById('zh-pending-list');
  el.innerHTML = myPending.length ? myPending.map(r=>reqCard(r,'zh')).join('') : emptyState('No pending requests. 🎉');

  const reviewed = zhReqs.filter(r=>r.status!=='pending').slice(0,3);
  document.getElementById('zh-recent-reviewed').innerHTML = reviewed.length ? reviewed.map(r=>reqCard(r,'zh')).join('') : emptyState('None reviewed yet.');
}

function renderZHHistory(filter='') {
  const reqs = REQUESTS.filter(r=>r.zhId===currentUser.id &&
    (r.agent.toLowerCase().includes(filter.toLowerCase()) || r.zone.toLowerCase().includes(filter.toLowerCase())));
  const el = document.getElementById('zh-history-list');
  el.innerHTML = reqs.length ? reqs.map(r=>reqCard(r,'zh')).join('') : emptyState('No requests found.');
}
function zhFilterHistory(v) { renderZHHistory(v); }

// ════════════════════════════════════════════
//  HO DASHBOARD
// ════════════════════════════════════════════
function renderHODashboard() {
  const dateInput = document.getElementById('ho-date-filter');
  if (dateInput) { dateInput.value = hoDashboardDate; }

  const filtered = REQUESTS.filter(r => r.effDate === hoDashboardDate);
  const allReqs = REQUESTS;

  // Count increased/decreased copies for selected date
  const increasedCount = filtered.filter(r=>r.type==='increase' && ['approved','erp_done'].includes(r.status))
    .reduce((s,r)=>s+(r.newSupply-r.oldSupply),0);
  const decreasedCount = filtered.filter(r=>r.type==='decrease' && ['approved','erp_done'].includes(r.status))
    .reduce((s,r)=>s+(r.oldSupply-r.newSupply),0);

  document.getElementById('ho-stat-increased').textContent = increasedCount;
  document.getElementById('ho-stat-decreased').textContent = decreasedCount;

  const pending = allReqs.filter(r=>r.status==='ho_review');
  const approved = allReqs.filter(r=>['approved','erp_done'].includes(r.status));
  const erp = allReqs.filter(r=>r.status==='erp_done');
  const rejected = allReqs.filter(r=>r.status==='rejected');
  document.getElementById('ho-stat-pending').textContent  = pending.length;
  document.getElementById('ho-stat-approved').textContent = approved.length;

  document.getElementById('ho-pending-count-badge').textContent = pending.length;

  const pEl = document.getElementById('ho-pending-list');
  pEl.innerHTML = pending.length ? pending.map(r=>reqCard(r,'ho')).join('') : emptyState('No pending HO approvals. 🎉');

  const recent = allReqs.filter(r=>r.status!=='ho_review').slice(0,3);
  document.getElementById('ho-recent-list').innerHTML = recent.length ? recent.map(r=>reqCard(r,'ho')).join('') : emptyState('None finalized yet.');
}

function hoSetDate(val) {
  hoDashboardDate = val;
  renderHODashboard();
}

function openHODetail(type) {
  // type = 'increase' or 'decrease'
  hoDetailFilter = { type, zone:'', branch:'' };
  renderHODetailScreen(type);
  setScreen('ho-detail');
}

function renderHODetailScreen(type) {
  const titleEl = document.getElementById('ho-detail-title');
  titleEl.textContent = type === 'increase' ? 'Increased Copies' : 'Decreased Copies';

  // Populate zone filter
  const zones = [...new Set(REQUESTS.map(r=>r.zone))];
  const zoneSelect = document.getElementById('ho-detail-zone-filter');
  zoneSelect.innerHTML = '<option value="">All Zones</option>' + zones.map(z=>`<option value="${z}">${z}</option>`).join('');
  zoneSelect.value = hoDetailFilter.zone || '';

  const branches = [...new Set(REQUESTS.map(r=>r.branch).filter(Boolean))];
  const branchSelect = document.getElementById('ho-detail-branch-filter');
  branchSelect.innerHTML = '<option value="">All Branches</option>' + branches.map(b=>`<option value="${b}">${b}</option>`).join('');
  branchSelect.value = hoDetailFilter.branch || '';

  applyHODetailFilters();
}

function applyHODetailFilters() {
  const type = hoDetailFilter.type;
  const zone = document.getElementById('ho-detail-zone-filter').value;
  const branch = document.getElementById('ho-detail-branch-filter').value;
  hoDetailFilter.zone = zone;
  hoDetailFilter.branch = branch;

  let reqs = REQUESTS.filter(r => r.type === type);
  if (zone) reqs = reqs.filter(r=>r.zone===zone);
  if (branch) reqs = reqs.filter(r=>r.branch===branch);

  const el = document.getElementById('ho-detail-list');
  el.innerHTML = reqs.length ? reqs.map(r => detailRowCard(r)).join('') : emptyState('No records found.');
}

function detailRowCard(r) {
  const diff = r.newSupply - r.oldSupply;
  const diffStr = (diff>0?'+':'')+diff;
  const icon = r.type==='increase'?'📈':'📉';
  return `<div class="req-item" onclick="showDetail('${r.id}','ho')">
    <div class="req-icon ${r.type}">${icon}</div>
    <div class="req-info">
      <div class="req-agent">${r.agent}</div>
      <div class="req-meta" style="display:flex;flex-wrap:wrap;gap:6px;margin-top:4px;">
        <span style="background:rgba(78,140,255,0.1);padding:2px 7px;border-radius:6px;font-size:11px;">${r.zone}</span>
        <span style="background:rgba(120,80,255,0.1);padding:2px 7px;border-radius:6px;font-size:11px;">${r.branch||'—'}</span>
        <span style="font-size:11px;color:var(--muted);">${r.effDate}</span>
      </div>
      <div class="req-by">${r.agentCode} · ${r.oldSupply}→${r.newSupply} (${diffStr})</div>
    </div>
    <div style="flex-shrink:0;"><span class="status-pill ${r.status}">${STATUS_LABELS[r.status]}</span></div>
  </div>`;
}

function renderHOHistory(filter='') {
  const reqs = REQUESTS.filter(r=>
    r.agent.toLowerCase().includes(filter.toLowerCase()) ||
    r.zone.toLowerCase().includes(filter.toLowerCase()) ||
    r.seName.toLowerCase().includes(filter.toLowerCase()));
  document.getElementById('ho-history-list').innerHTML = reqs.length ? reqs.map(r=>reqCard(r,'ho')).join('') : emptyState('No requests found.');
}
function hoFilterHistory(v) { renderHOHistory(v); }

function renderHOReports() {
  // Date filter
  const dateInput = document.getElementById('report-date-filter');
  const selectedDate = dateInput ? dateInput.value : '';

  let reqs = selectedDate ? REQUESTS.filter(r=>r.effDate===selectedDate) : REQUESTS;

  // Branch wise: branch name + increased + decreased counts
  const branches = {};
  reqs.forEach(r => {
    const b = r.branch || r.zone;
    if (!branches[b]) branches[b] = { increased:0, decreased:0 };
    if (r.type==='increase') branches[b].increased += (r.newSupply - r.oldSupply);
    else branches[b].decreased += (r.oldSupply - r.newSupply);
  });

  let bHTML = '';
  Object.entries(branches).forEach(([b,d]) => {
    bHTML += `<div class="report-branch-row" onclick="openReportDetail('${b}','${selectedDate}')">
      <div style="font-size:13px;font-weight:700;">${b}</div>
      <div style="display:flex;gap:10px;margin-top:4px;">
        <span style="color:var(--green);font-size:12px;">📈 +${d.increased}</span>
        <span style="color:var(--accent);font-size:12px;">📉 -${d.decreased}</span>
      </div>
      <div style="font-size:11px;color:var(--blue);margin-top:3px;">View Details →</div>
    </div>`;
  });
  document.getElementById('ho-branch-summary').innerHTML = bHTML || emptyState('No data for selected date.');

  // ERP log only
  const erp = reqs.filter(r=>r.status==='erp_done');
  let lHTML = erp.length ? erp.map(r=>`<div style="font-size:12px;color:var(--muted);padding:8px;background:rgba(42,157,143,0.06);border-radius:8px;border-left:3px solid var(--green);">
    <b style="color:var(--paper);">${r.agentCode}</b> · ${r.agent} · ${r.oldSupply}→${r.newSupply} <span style="color:var(--green);">✅ Pushed</span>
  </div>`).join('') : emptyState('No ERP pushes yet.');
  document.getElementById('ho-erp-log').innerHTML = lHTML;
}

function setReportDate(val) { renderHOReports(); }

function openReportDetail(branch, date) {
  hoDetailFilter = { type:'all', zone:'', branch };
  document.getElementById('ho-detail-title').textContent = `Branch: ${branch}`;
  const zones = [...new Set(REQUESTS.map(r=>r.zone))];
  const zoneSelect = document.getElementById('ho-detail-zone-filter');
  zoneSelect.innerHTML = '<option value="">All Zones</option>' + zones.map(z=>`<option value="${z}">${z}</option>`).join('');
  const branchSelect = document.getElementById('ho-detail-branch-filter');
  const branches = [...new Set(REQUESTS.map(r=>r.branch).filter(Boolean))];
  branchSelect.innerHTML = '<option value="">All Branches</option>' + branches.map(b=>`<option value="${b}">${b}</option>`).join('');
  branchSelect.value = branch;

  let reqs = REQUESTS.filter(r=>(r.branch===branch));
  if (date) reqs = reqs.filter(r=>r.effDate===date);
  const el = document.getElementById('ho-detail-list');
  el.innerHTML = reqs.length ? reqs.map(r=>detailRowCard(r)).join('') : emptyState('No records for this branch.');
  setScreen('ho-detail');
}

// ════════════════════════════════════════════
//  REQUEST CARD HTML  — with Agency, Branch, Approve icon in summary
// ════════════════════════════════════════════
const STATUS_LABELS = {
  pending:'Pending ZH', approved:'Approved', rejected:'Rejected',
  processing:'Processing', ho_review:'At HO', erp_done:'ERP Done'
};

function reqCard(r, viewRole) {
  const diff = r.newSupply - r.oldSupply;
  const diffStr = (diff>0?'+':'')+diff;
  const icon = r.type==='increase'?'📈':'📉';

  // Approver icon for ZH (pending) and HO (ho_review) — quick approve
  let approveBtn = '';
  if (viewRole==='zh' && r.status==='pending') {
    approveBtn = `<button class="quick-approve-btn" onclick="event.stopPropagation();quickApprove('zh','${r.id}')" title="Approve">✅</button>`;
  } else if (viewRole==='ho' && r.status==='ho_review') {
    approveBtn = `<button class="quick-approve-btn" onclick="event.stopPropagation();quickApprove('ho','${r.id}')" title="Approve">✅</button>`;
  }

  return `<div class="req-item" onclick="showDetail('${r.id}','${viewRole}')">
    <div class="req-icon ${r.type}">${icon}</div>
    <div class="req-info">
      <div class="req-agent">${r.agent}</div>
      <div class="req-meta" style="display:flex;flex-wrap:wrap;gap:6px;margin-top:3px;">
        <span style="background:rgba(78,140,255,0.1);padding:2px 7px;border-radius:6px;font-size:11px;">${r.zone}</span>
        ${r.branch?`<span style="background:rgba(120,80,255,0.1);padding:2px 7px;border-radius:6px;font-size:11px;">${r.branch}</span>`:''}
        <span style="font-size:11px;color:var(--muted);">${r.effDate}</span>
      </div>
      <div class="req-by">${r.agentCode} · By ${r.seName}</div>
    </div>
    <div style="flex-shrink:0;display:flex;flex-direction:column;gap:6px;align-items:flex-end;">
      <span class="status-pill ${r.status}">${STATUS_LABELS[r.status]}</span>
      ${approveBtn}
    </div>
  </div>`;
}

function emptyState(msg) {
  return `<div class="empty-state"><div class="empty-icon">📭</div><p>${msg}</p></div>`;
}

// Quick approve from list view
function quickApprove(role, reqId) {
  pendingRemarksAction = { action: role==='zh'?'zh_approve':'ho_approve', reqId };
  const r = REQUESTS.find(x=>x.id===reqId);
  if (!r) return;
  document.getElementById('remarks-modal-title').textContent = role==='zh'?'Approve & Forward to HO':'Final Approval';
  document.getElementById('remarks-modal-label').textContent = 'Remarks (optional)';
  document.getElementById('remarks-modal-input').value='';
  const btn = document.getElementById('remarks-confirm-btn');
  btn.textContent = role==='zh'?'Forward to HO →':'Approve & Push ERP';
  btn.className = 'btn btn-approve';
  document.getElementById('remarks-modal').classList.add('show');
}

// ════════════════════════════════════════════
//  DETAIL MODAL
// ════════════════════════════════════════════
function showDetail(id, viewRole) {
  activeDetailId = id;
  const r = REQUESTS.find(x=>x.id===id);
  if (!r) return;
  const diff = r.newSupply - r.oldSupply;
  const diffStr = (diff>0?'+':'')+diff;

  // Highlighted Reason & Remark fields
  const remarksSection = `
    <div class="card-title" style="margin-top:14px;">Reason & Remarks</div>
    <div class="detail-highlight-field reason-highlight">
      <span class="detail-field-label">Reason</span>
      <span class="detail-field-value">${r.reason}</span>
    </div>
    <div class="detail-highlight-field remark-highlight" style="margin-top:8px;">
      <span class="detail-field-label">Remarks</span>
      <span class="detail-field-value">${r.remarks || '—'}</span>
    </div>
    ${r.zhRemarks?`<div style="font-size:12px;color:var(--muted);line-height:1.6;margin-top:8px;"><b style="color:var(--purple);">ZH Remarks:</b> ${r.zhRemarks}</div>`:''}
    ${r.hoRemarks?`<div style="font-size:12px;color:var(--muted);line-height:1.6;margin-top:5px;"><b style="color:var(--accent);">HO Remarks:</b> ${r.hoRemarks}</div>`:''}
  `;

  document.getElementById('modal-title').textContent = 'Request #'+r.id;
  document.getElementById('modal-body').innerHTML = `
    <div style="display:flex;gap:8px;margin-bottom:14px;align-items:center;flex-wrap:wrap;">
      <span class="status-pill ${r.status}" style="font-size:11px;padding:5px 12px;">${STATUS_LABELS[r.status]}</span>
      <span style="font-size:11px;color:var(--muted);">${r.effDate} · ${r.submittedAt}</span>
    </div>
    <div class="supply-diff" style="margin-bottom:14px;">
      <div style="text-align:center"><div class="supply-num">${r.oldSupply}</div><div class="supply-sub">Current</div></div>
      <div class="arrow">${r.type==='increase'?'📈':'📉'}</div>
      <div style="text-align:center"><div class="supply-num ${r.type}">${r.newSupply}</div><div class="supply-sub">New</div></div>
      <div style="text-align:center"><div class="supply-num" style="font-size:18px;color:${diff>0?'var(--green)':'var(--accent)'}">${diffStr}</div><div class="supply-sub">Diff</div></div>
    </div>
    <div style="font-size:12px;line-height:2;margin-bottom:12px;">
      <span style="color:var(--muted)">Agency:</span> <b>${r.agent}</b> (${r.agentCode})<br>
      <span style="color:var(--muted)">Zone:</span> <b>${r.zone}</b><br>
      <span style="color:var(--muted)">Branch:</span> <b>${r.branch||'—'}</b><br>
      <span style="color:var(--muted)">Effective Date:</span> <b>${r.effDate}</b><br>
      <span style="color:var(--muted)">By:</span> ${r.seName}
    </div>
    ${remarksSection}
  `;

  // Role-specific action buttons — NO pipeline shown
  let actHTML = '';
  if (viewRole==='zh' && r.status==='pending') {
    actHTML = `<div class="action-row">
      <button class="btn btn-danger btn-icon btn-sm" style="width:100%;padding:12px;" onclick="openRemarksModal('zh_reject','${r.id}')">❌ Reject</button>
      <button class="btn btn-approve btn-icon btn-sm" style="width:100%;padding:12px;" onclick="openRemarksModal('zh_approve','${r.id}')">✅ Approve</button>
    </div>`;
  } else if (viewRole==='ho' && r.status==='ho_review') {
    actHTML = `<div class="action-row">
      <button class="btn btn-danger btn-icon btn-sm" style="width:100%;padding:12px;" onclick="openRemarksModal('ho_reject','${r.id}')">❌ Reject</button>
      <button class="btn btn-approve btn-icon btn-sm" style="width:100%;padding:12px;" onclick="openRemarksModal('ho_approve','${r.id}')">✅ Approve & Push ERP</button>
    </div>`;
  } else if (viewRole==='ho' && r.status==='approved') {
    actHTML = `<button class="btn btn-blue btn-icon" onclick="pushToERP('${r.id}')"><span>🔄</span><span>Push to ERP Now</span></button>`;
  }
  document.getElementById('modal-actions').innerHTML = actHTML;
  document.getElementById('detail-modal').classList.add('show');
}

function closeModalOutside(e, id) {
  if (e.target.id === id) closeModal(id);
}
function closeModal(id) {
  document.getElementById(id).classList.remove('show');
}

// ════════════════════════════════════════════
//  APPROVAL ACTIONS
// ════════════════════════════════════════════
function openRemarksModal(action, reqId) {
  pendingRemarksAction = { action, reqId };
  const labels = {
    zh_approve: { title:'Approve & Forward to HO', label:'Remarks for HO (optional)', btn:'Approve →', cls:'btn-approve' },
    zh_reject:  { title:'Reject Request', label:'Rejection reason *', btn:'Confirm Reject', cls:'btn-danger' },
    ho_approve: { title:'Final Approval', label:'Approval remarks (optional)', btn:'Approve & Push ERP', cls:'btn-approve' },
    ho_reject:  { title:'Reject Request', label:'Rejection reason *', btn:'Confirm Reject', cls:'btn-danger' },
  };
  const lbl = labels[action];
  document.getElementById('remarks-modal-title').textContent = lbl.title;
  document.getElementById('remarks-modal-label').textContent = lbl.label;
  document.getElementById('remarks-modal-input').value = '';
  const btn = document.getElementById('remarks-confirm-btn');
  btn.textContent = lbl.btn;
  btn.className = 'btn '+lbl.cls;
  closeModal('detail-modal');
  document.getElementById('remarks-modal').classList.add('show');
}

function confirmRemarksAction() {
  if (!pendingRemarksAction) return;
  const { action, reqId } = pendingRemarksAction;
  const remarks = document.getElementById('remarks-modal-input').value.trim();
  const req = REQUESTS.find(r=>r.id===reqId);
  if (!req) return;

  if (action==='zh_approve') {
    req.status='ho_review'; req.pipelineStep=3; req.zhRemarks=remarks||'Approved by Zonal Head.';
    showToast('✅ Approved & forwarded to Jaipur HO!');
    closeModal('remarks-modal');
    renderZHDashboard();
  } else if (action==='zh_reject') {
    if (!remarks) { showToast('⚠️ Please enter rejection reason'); return; }
    req.status='rejected'; req.pipelineStep=2; req.zhRemarks='Rejected: '+remarks;
    showToast('Request rejected.');
    closeModal('remarks-modal');
    renderZHDashboard();
  } else if (action==='ho_approve') {
    req.status='approved'; req.pipelineStep=4; req.hoRemarks=remarks||'Approved by HO.';
    showToast('✅ Approved! Queued for ERP at cut-off.');
    closeModal('remarks-modal');
    renderHODashboard();
    setTimeout(()=>{ pushToERP(reqId, true); }, 2000);
  } else if (action==='ho_reject') {
    if (!remarks) { showToast('⚠️ Please enter rejection reason'); return; }
    req.status='rejected'; req.pipelineStep=3; req.hoRemarks='Rejected: '+remarks;
    showToast('Request rejected.');
    closeModal('remarks-modal');
    renderHODashboard();
  }
  pendingRemarksAction = null;
}

function pushToERP(reqId, auto=false) {
  const req = REQUESTS.find(r=>r.id===reqId);
  if (!req) return;
  req.status='erp_done'; req.pipelineStep=5;
  if (ERP_AGENTS[req.agentCode]) ERP_AGENTS[req.agentCode].supply = req.newSupply;
  closeModal('detail-modal');
  showToast('🔄 ERP Updated! Supply changed to '+req.newSupply);
  renderHODashboard();
}

// ════════════════════════════════════════════
//  NEW REQUEST FORM (SE)
// ════════════════════════════════════════════
let agentLookupTimer = null;

function initNewRequestForm() {
  const todayStr = today();
  document.getElementById('eff-date').value = todayStr;
  document.getElementById('eff-date').min = todayStr;  // No back-date allowed
}

function lookupAgent(code) {
  document.getElementById('agent-name').value = '';
  document.getElementById('agent-zone-disp').value = '';
  document.getElementById('old-supply').value = '';
  document.getElementById('erp-loading').style.display='none';
  document.getElementById('erp-ok').style.display='none';
  calcDiff();
  clearTimeout(agentLookupTimer);
  const trimmed = code.toUpperCase().trim();
  if (trimmed.length < 5) return;
  document.getElementById('erp-loading').style.display='block';
  agentLookupTimer = setTimeout(()=>{
    const agent = ERP_AGENTS[trimmed];
    document.getElementById('erp-loading').style.display='none';
    if (agent) {
      if (agent.se !== currentUser.id) {
        showToast('⚠️ Agency not under your territory');
        return;
      }
      document.getElementById('agent-name').value = agent.name;
      document.getElementById('agent-zone-disp').value = agent.zone;
      document.getElementById('old-supply').value = agent.supply;
      document.getElementById('erp-ok').style.display='block';
      calcDiff();
      showToast('✅ ERP: '+agent.name+' · Current supply: '+agent.supply);
    } else {
      showToast('❌ Agency code not found in ERP');
    }
  }, 800);
}

function selectType(type) {
  selectedType = type;
  document.getElementById('chip-increase').classList.toggle('active', type==='increase');
  document.getElementById('chip-decrease').classList.toggle('active', type==='decrease');
  calcDiff();
}

function calcDiff() {
  const oldV = parseInt(document.getElementById('old-supply').value)||0;
  const newV = parseInt(document.getElementById('new-supply').value)||0;
  const diff = newV - oldV;
  document.getElementById('prev-old').textContent = oldV||'—';
  document.getElementById('prev-new').textContent = newV||'—';
  if (oldV && newV) {
    document.getElementById('prev-diff').textContent = (diff>0?'+':'')+diff;
    document.getElementById('prev-diff').className = 'supply-num '+(diff>0?'increase':'decrease');
    document.getElementById('prev-diff').style.fontSize = '18px';
    document.getElementById('prev-new').className = 'supply-num '+(diff>0?'increase':'decrease');
  } else {
    document.getElementById('prev-diff').textContent='—';
    document.getElementById('prev-diff').className='supply-num';
    document.getElementById('prev-new').className='supply-num';
  }
}

function toggleOtherReason() {
  document.getElementById('other-reason-field').style.display =
    document.getElementById('reason-select').value==='Other' ? 'block' : 'none';
}

function submitRequest() {
  const agentCode = document.getElementById('agent-code').value.trim().toUpperCase();
  const agentName = document.getElementById('agent-name').value.trim();
  const zone = document.getElementById('agent-zone-disp').value;
  const oldS = document.getElementById('old-supply').value;
  const newS = document.getElementById('new-supply').value;
  const reason = document.getElementById('reason-select').value;
  const effDate = document.getElementById('eff-date').value;
  const remarksVal = document.getElementById('remarks').value.trim();

  if (!agentCode || !agentName) { showToast('⚠️ Enter valid agency code'); return; }
  if (!oldS || !newS) { showToast('⚠️ Supply details required'); return; }
  if (parseInt(newS) === parseInt(oldS)) { showToast('⚠️ New supply must differ'); return; }
  if (!reason) { showToast('⚠️ Select a reason'); return; }
  if (!effDate) { showToast('⚠️ Select effective date'); return; }
  if (effDate < today()) { showToast('⚠️ Back-date not allowed'); return; }
  if (reason==='Other' && !document.getElementById('other-reason').value.trim()) {
    showToast('⚠️ Specify the reason'); return;
  }

  const agent = ERP_AGENTS[agentCode];
  const branch = agent ? agent.branch : '';
  const zh = ERP_USERS.zh.find(z=>z.id===currentUser.zoneHead);
  const newReq = {
    id: 'REQ' + String(REQUESTS.length+1).padStart(3,'0'),
    agentCode, agent:agentName, zone, branch, seId:currentUser.id, seName:currentUser.name,
    zhId: currentUser.zoneHead,
    type: parseInt(newS)>parseInt(oldS)?'increase':'decrease',
    oldSupply:parseInt(oldS), newSupply:parseInt(newS),
    reason: reason==='Other'?document.getElementById('other-reason').value:reason,
    remarks: remarksVal, effDate,
    status:'pending', pipelineStep:2, submittedAt: new Date().toLocaleTimeString('en-IN',{hour:'2-digit',minute:'2-digit'}),
    zhRemarks:'', hoRemarks:''
  };
  REQUESTS.unshift(newReq);

  ['agent-code','agent-name','agent-zone-disp','old-supply','new-supply','remarks','other-reason'].forEach(id=>{
    document.getElementById(id).value='';
  });
  document.getElementById('reason-select').value='';
  document.getElementById('other-reason-field').style.display='none';
  document.getElementById('erp-ok').style.display='none';
  ['prev-old','prev-new','prev-diff'].forEach(id=>document.getElementById(id).textContent='—');
  document.getElementById('prev-new').className='supply-num';
  document.getElementById('prev-diff').className='supply-num';

  showToast('🚀 Request submitted! Sent to '+(zh?zh.name:'Zonal Head'));
  setTimeout(()=>seNav('dashboard'), 1300);
}

// ════════════════════════════════════════════
//  TOAST
// ════════════════════════════════════════════
let toastTimeout;
function showToast(msg) {
  const t = document.getElementById('toast');
  t.textContent = msg;
  t.classList.add('show');
  clearTimeout(toastTimeout);
  toastTimeout = setTimeout(()=>t.classList.remove('show'), 3200);
}

// ════════════════════════════════════════════
//  INIT
// ════════════════════════════════════════════
initNewRequestForm();

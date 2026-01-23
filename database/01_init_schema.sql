-- Documentation Completeness Agent Schema
-- Extracted from Requirements Document

-- 1. Repositories Table
CREATE TABLE repositories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    source VARCHAR(50) NOT NULL, -- 'github' or 'ado'
    repository_url TEXT NOT NULL,
    default_branch VARCHAR(100) DEFAULT 'main',
    is_active BOOLEAN DEFAULT true,
    last_scanned_at TIMESTAMP,
    scan_frequency VARCHAR(50), -- 'manual', 'daily', 'weekly', 'on-commit'
    settings JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(255),
    CONSTRAINT unique_repo_url UNIQUE(repository_url)
);

CREATE INDEX idx_repositories_source ON repositories(source);
CREATE INDEX idx_repositories_active ON repositories(is_active);
CREATE INDEX idx_repositories_last_scanned ON repositories(last_scanned_at);

-- 2. Analysis Jobs Table
CREATE TABLE analysis_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    repository_id UUID NOT NULL REFERENCES repositories(id) ON DELETE CASCADE,
    status VARCHAR(50) NOT NULL, -- 'queued', 'running', 'completed', 'failed', 'cancelled'
    scan_type VARCHAR(50), -- 'full', 'incremental', 'pr'
    branch VARCHAR(100),
    commit_sha VARCHAR(100),
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    duration_seconds INTEGER,
    error_message TEXT,
    configuration JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(255),
    CONSTRAINT chk_status CHECK (status IN ('queued', 'running', 'completed', 'failed', 'cancelled'))
);

CREATE INDEX idx_analysis_jobs_repository ON analysis_jobs(repository_id);
CREATE INDEX idx_analysis_jobs_status ON analysis_jobs(status);
CREATE INDEX idx_analysis_jobs_created ON analysis_jobs(created_at DESC);

-- 3. Analysis Results Table
CREATE TABLE analysis_results (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id UUID NOT NULL REFERENCES analysis_jobs(id) ON DELETE CASCADE,
    repository_id UUID NOT NULL REFERENCES repositories(id) ON DELETE CASCADE,
    total_files INTEGER NOT NULL,
    analyzed_files INTEGER NOT NULL,
    skipped_files INTEGER NOT NULL,
    overall_coverage DECIMAL(5,2),
    total_gaps INTEGER NOT NULL,
    critical_gaps INTEGER DEFAULT 0,
    high_priority_gaps INTEGER DEFAULT 0,
    medium_priority_gaps INTEGER DEFAULT 0,
    low_priority_gaps INTEGER DEFAULT 0,
    coverage_by_type JSONB, -- { "classes": 72.3, "methods": 65.1, ... }
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_analysis_results_job ON analysis_results(job_id);
CREATE INDEX idx_analysis_results_repository ON analysis_results(repository_id);
CREATE INDEX idx_analysis_results_coverage ON analysis_results(overall_coverage);

-- 4. Documentation Gaps Table
CREATE TABLE documentation_gaps (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_id UUID NOT NULL REFERENCES analysis_jobs(id) ON DELETE CASCADE,
    repository_id UUID NOT NULL REFERENCES repositories(id) ON DELETE CASCADE,
    file_path TEXT NOT NULL,
    file_type VARCHAR(50), -- 'csharp', 'typescript', 'python', 'markdown'
    line_number INTEGER,
    element_name VARCHAR(500), -- class name, method name, etc.
    element_type VARCHAR(50), -- 'class', 'method', 'property', 'file', 'readme'
    gap_type VARCHAR(100), -- 'missing', 'incomplete', 'outdated', 'incorrect_format'
    severity VARCHAR(20) NOT NULL, -- 'critical', 'high', 'medium', 'low'
    message TEXT,
    current_documentation TEXT,
    ai_suggestion TEXT,
    suggestion_confidence DECIMAL(3,2), -- 0.00 to 1.00
    status VARCHAR(50) DEFAULT 'open', -- 'open', 'resolved', 'ignored', 'in_progress'
    resolved_at TIMESTAMP,
    resolved_by VARCHAR(255),
    work_item_id VARCHAR(100), -- ADO work item ID if created
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_severity CHECK (severity IN ('critical', 'high', 'medium', 'low')),
    CONSTRAINT chk_status CHECK (status IN ('open', 'resolved', 'ignored', 'in_progress'))
);

CREATE INDEX idx_documentation_gaps_job ON documentation_gaps(job_id);
CREATE INDEX idx_documentation_gaps_repository ON documentation_gaps(repository_id);
CREATE INDEX idx_documentation_gaps_status ON documentation_gaps(status);
CREATE INDEX idx_documentation_gaps_severity ON documentation_gaps(severity);
CREATE INDEX idx_documentation_gaps_file ON documentation_gaps(file_path);
CREATE INDEX idx_documentation_gaps_element ON documentation_gaps(element_type, element_name);

-- 5. Documentation Templates Table
CREATE TABLE documentation_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    language VARCHAR(50), -- 'csharp', 'typescript', 'python', 'all'
    element_type VARCHAR(50), -- 'class', 'method', 'property', 'file'
    template_content TEXT NOT NULL,
    variables JSONB, -- Variables that can be substituted in template
    is_default BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(255)
);

CREATE INDEX idx_templates_language ON documentation_templates(language);
CREATE INDEX idx_templates_element_type ON documentation_templates(element_type);
CREATE INDEX idx_templates_active ON documentation_templates(is_active);

-- 6. Coverage History Table
CREATE TABLE coverage_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    repository_id UUID NOT NULL REFERENCES repositories(id) ON DELETE CASCADE,
    job_id UUID NOT NULL REFERENCES analysis_jobs(id) ON DELETE CASCADE,
    measured_at TIMESTAMP NOT NULL,
    overall_coverage DECIMAL(5,2),
    class_coverage DECIMAL(5,2),
    method_coverage DECIMAL(5,2),
    property_coverage DECIMAL(5,2),
    file_coverage DECIMAL(5,2),
    total_gaps INTEGER,
    critical_gaps INTEGER,
    high_priority_gaps INTEGER,
    medium_priority_gaps INTEGER,
    low_priority_gaps INTEGER,
    CONSTRAINT unique_repo_measurement UNIQUE(repository_id, measured_at)
);

CREATE INDEX idx_coverage_history_repository ON coverage_history(repository_id);
CREATE INDEX idx_coverage_history_measured ON coverage_history(measured_at DESC);

-- 7. Users Table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    full_name VARCHAR(255),
    ado_user_id VARCHAR(100),
    github_username VARCHAR(100),
    role VARCHAR(50) DEFAULT 'developer', -- 'admin', 'manager', 'developer'
    notification_preferences JSONB,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);

-- 8. Notifications Table
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    notification_type VARCHAR(50), -- 'email', 'teams', 'slack', 'in_app'
    subject VARCHAR(500),
    message TEXT,
    severity VARCHAR(20), -- 'info', 'warning', 'critical'
    related_entity_type VARCHAR(50), -- 'repository', 'gap', 'job'
    related_entity_id UUID,
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'sent', 'failed', 'read'
    sent_at TIMESTAMP,
    read_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_notifications_user ON notifications(user_id);
CREATE INDEX idx_notifications_status ON notifications(status);
CREATE INDEX idx_notifications_created ON notifications(created_at DESC);

-- 9. ADO Work Items Table
CREATE TABLE ado_work_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    gap_id UUID REFERENCES documentation_gaps(id) ON DELETE CASCADE,
    work_item_id INTEGER NOT NULL, -- ADO work item ID
    work_item_url TEXT,
    work_item_type VARCHAR(50), -- 'Task', 'Bug', 'User Story'
    state VARCHAR(50), -- 'New', 'Active', 'Resolved', 'Closed'
    assigned_to VARCHAR(255),
    created_in_ado_at TIMESTAMP,
    synced_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT unique_gap_work_item UNIQUE(gap_id)
);

CREATE INDEX idx_ado_work_items_gap ON ado_work_items(gap_id);
CREATE INDEX idx_ado_work_items_state ON ado_work_items(state);

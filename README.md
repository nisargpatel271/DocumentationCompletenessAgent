# Documentation Completeness Agent

## Overview
An AI-powered agent to analyze code repositories, identify insufficient documentation, and generate drafts to fill the gaps.

## Project Structure
- `src/web`: Frontend (React + TypeScript + Vite)
- `src/api`: Backend (.NET 8 Web API) - *Requires .NET 8 SDK*
- `database`: SQL Scripts & Migrations (PostgreSQL)

## Getting Started

### Prerequisites
- Node.js (v18+)
- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL (or use Docker)

### Running Locally

#### Frontend
```bash
cd src/web
npm install
npm run dev
```

#### Backend
*(Instructions pending .NET Setup)*

#### Database
Run the scripts in `database/` to initialize your PostgreSQL instance.

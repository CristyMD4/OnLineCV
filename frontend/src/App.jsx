import { useEffect, useMemo, useRef, useState } from 'react'
import { API_BASE_URL } from './api.js'

const EMPTY_LIST = []

function App() {
  const isMaintenancePreview = new URLSearchParams(window.location.search).get('maintenance') === '1'
  const [cvContent, setCvContent] = useState(null)
  const [loadState, setLoadState] = useState(isMaintenancePreview ? 'error' : 'loading')
  const [retryCount, setRetryCount] = useState(0)
  const [selectedProjectId, setSelectedProjectId] = useState('')
  const [isHeaderHidden, setIsHeaderHidden] = useState(false)
  const lastScrollY = useRef(0)
  const ticking = useRef(false)
  const projects = cvContent?.projects ?? EMPTY_LIST
  const selectedProject = useMemo(
    () => projects.find((project) => project.id === selectedProjectId) ?? projects[0],
    [projects, selectedProjectId],
  )

  useEffect(() => {
    if (isMaintenancePreview) {
      document.title = 'OnlineCV - Under maintenance'
      return undefined
    }

    const controller = new AbortController()

    async function loadCvContent() {
      setLoadState('loading')

      try {
        const response = await fetch(`${API_BASE_URL}/api/cv`, {
          cache: 'no-store',
          signal: controller.signal,
        })

        if (!response.ok) {
          throw new Error(`CV API returned ${response.status}`)
        }

        const content = await response.json()
        if (!content?.identity || !content?.uiText || !content?.projects?.length) {
          throw new Error('CV API returned incomplete content')
        }

        setCvContent(content)
        setLoadState('ready')
        setSelectedProjectId(content.projects[0]?.id ?? '')
        document.title = content.uiText.pageTitle

        const description = document.querySelector('meta[name="description"]')
        description?.setAttribute('content', content.uiText.pageDescription)
      } catch (error) {
        if (error.name !== 'AbortError') {
          console.error('Could not load CV content from the API.', error)
          setCvContent(null)
          setLoadState('error')
          document.title = 'OnlineCV - Under maintenance'
        }
      }
    }

    loadCvContent()

    return () => controller.abort()
  }, [isMaintenancePreview, retryCount])

  useEffect(() => {
    function updateHeaderVisibility() {
      const currentScrollY = window.scrollY
      const scrollDelta = currentScrollY - lastScrollY.current

      if (currentScrollY <= 24) {
        setIsHeaderHidden(false)
      } else if (scrollDelta > 4) {
        setIsHeaderHidden(true)
      } else if (scrollDelta < -4) {
        setIsHeaderHidden(false)
      }

      lastScrollY.current = currentScrollY
      ticking.current = false
    }

    function handleScroll() {
      if (!ticking.current) {
        window.requestAnimationFrame(updateHeaderVisibility)
        ticking.current = true
      }
    }

    lastScrollY.current = window.scrollY
    window.addEventListener('scroll', handleScroll, { passive: true })

    return () => {
      window.removeEventListener('scroll', handleScroll)
    }
  }, [])

  function handleProjectSelect(projectId) {
    setSelectedProjectId(projectId)
  }

  function handleRetry() {
    if (isMaintenancePreview) {
      window.location.assign('/')
      return
    }

    setRetryCount((count) => count + 1)
  }

  if (loadState === 'error') {
    return (
      <main className="service-page">
        <section className="service-message" aria-labelledby="service-title" role="status">
          <div className="service-status-row">
            <a className="service-brand" href="/">OnlineCV</a>
            <span>Temporarily unavailable</span>
          </div>
          <div className="service-code" aria-hidden="true">503</div>
          <p className="service-eyebrow">Under maintenance</p>
          <h1 id="service-title">We're sorry, the site is under repair.</h1>
          <p className="service-description">
            We're making a few improvements and the CV cannot be loaded right now.
            Please try again shortly.
          </p>
          <button type="button" onClick={handleRetry}>
            {isMaintenancePreview ? 'Return to site' : 'Try again'}
          </button>
          <p className="service-note">Your browser is working correctly. The service is currently offline.</p>
        </section>
      </main>
    )
  }

  if (loadState === 'loading' || !cvContent || !selectedProject) {
    return (
      <main className="service-page" aria-busy="true">
        <div className="service-loading" role="status">
          <span aria-hidden="true" />
          Loading CV
        </div>
      </main>
    )
  }

  const {
    achievements,
    collaboration,
    contactMethods,
    credentials,
    engineeringStandards,
    experience,
    identity,
    languages,
    metrics,
    navigation,
    professionalDetails,
    roleFit,
    roleSignals,
    skillGroups,
    stackMatrix,
    strengths,
    summaryHighlights,
    tools,
    uiText,
    workflow,
  } = cvContent

  return (
    <main className="cv-page">
      <header
        aria-hidden={isHeaderHidden}
        aria-label={uiText.pageNavigationLabel}
        className={`topbar${isHeaderHidden ? ' topbar--hidden' : ''}`}
        inert={isHeaderHidden ? true : undefined}
      >
        <a className="brand" href="#profile">
          {identity.name}
        </a>
        <nav>
          {navigation.map((item) => (
            <a href={item.href} key={item.href}>
              {item.label}
            </a>
          ))}
        </nav>
        <div className="topbar-actions">
          <button type="button" onClick={() => window.print()}>
            {uiText.downloadPdf}
          </button>
          <button type="button" onClick={() => window.print()}>
            {uiText.printCv}
          </button>
        </div>
      </header>

      <section className="hero-section" id="profile" aria-labelledby="candidate-name">
        <div className="hero-copy">
          <div className="summary-strip" aria-label={uiText.professionalHighlightsLabel}>
            {summaryHighlights.map((item) => (
              <span key={item}>{item}</span>
            ))}
          </div>
          <p className="eyebrow">{uiText.heroEyebrow}</p>
          <h1 id="candidate-name">{identity.name}</h1>
          <p className="role">{identity.role}</p>
          <p className="intro">{identity.summary}</p>
          <div className="hero-actions" aria-label={uiText.contactLinksLabel}>
            <a href={`mailto:${identity.email}`}>{identity.email}</a>
            <a href={`tel:${identity.phone.replaceAll(' ', '')}`}>{identity.phone}</a>
            <a href={identity.github} target="_blank" rel="noreferrer">
              {uiText.githubLabel}
            </a>
            <a href={identity.linkedin} target="_blank" rel="noreferrer">
              {uiText.linkedinLabel}
            </a>
          </div>
          <div className="role-signal-grid" aria-label={uiText.roleFocusLabel}>
            {roleSignals.map((signal) => (
              <article key={signal.title}>
                <h2>{signal.title}</h2>
                <p>{signal.text}</p>
              </article>
            ))}
          </div>
        </div>

        <aside className="profile-panel" aria-label={uiText.careerSnapshotLabel}>
          <div className="panel-header">
            <span>{uiText.developerProfile}</span>
            <strong>{uiText.open}</strong>
          </div>
          <div className="avatar-card" aria-label={uiText.candidateAvatarLabel}>
            <div className="avatar-mark" aria-hidden="true">
              {uiText.avatarInitials}
            </div>
            <div>
              <h2>{identity.name}</h2>
              <p>{identity.role}</p>
            </div>
          </div>
          <div className="profile-summary">
            <p>{uiText.profileSummary}</p>
          </div>
          <div className="availability-panel">
            <span>{identity.availability}</span>
            <small>{identity.workMode}</small>
          </div>
          <div className="metric-grid">
            {metrics.map((metric) => (
              <div className="metric-card" key={metric.label}>
                <span className="metric">{metric.value}</span>
                <span className="metric-label">{metric.label}</span>
              </div>
            ))}
          </div>
        </aside>
      </section>

      <section className="systems-row" aria-label={uiText.capabilityOverviewLabel}>
        {skillGroups.map((group) => (
          <article className="system-card" key={group.title}>
            <div>
              <h2>{group.score}</h2>
              <span>{group.title}</span>
            </div>
            <meter min="0" max="100" value={group.score}>
              {group.score}
            </meter>
          </article>
        ))}
      </section>

      <section className="content-grid">
        <div className="main-column">
          <section className="section-block" id="impact" aria-labelledby="impact-heading">
            <div className="section-heading split-heading">
              <div>
                <p>{uiText.impactEyebrow}</p>
                <h2 id="impact-heading">{uiText.impactTitle}</h2>
              </div>
              <span>{uiText.impactSummary}</span>
            </div>
            <div className="achievement-grid">
              {achievements.map((achievement, index) => (
                <article className="achievement-card" key={achievement.title}>
                  <span>{String(index + 1).padStart(2, '0')}</span>
                  <h3>{achievement.title}</h3>
                  <p>{achievement.detail}</p>
                </article>
              ))}
            </div>
          </section>

          <section className="section-block strengths-section" aria-labelledby="strengths-heading">
            <div className="section-heading split-heading">
              <div>
                <p>{uiText.strengthsEyebrow}</p>
                <h2 id="strengths-heading">{uiText.strengthsTitle}</h2>
              </div>
              <span>{uiText.strengthsSummary}</span>
            </div>
            <div className="strength-grid">
              {strengths.map((strength) => (
                <article className="strength-card" key={strength.title}>
                  <h3>{strength.title}</h3>
                  <p>{strength.text}</p>
                </article>
              ))}
            </div>
          </section>

          <section className="section-block project-lab" id="projects" aria-labelledby="projects-heading">
            <div className="section-heading split-heading">
              <div>
                <p>{uiText.caseStudiesEyebrow}</p>
                <h2 id="projects-heading">{uiText.projectsTitle}</h2>
              </div>
              <span>{uiText.projectsSummary}</span>
            </div>

            <div className="project-switcher" role="group" aria-label={uiText.projectCaseStudiesLabel}>
              {projects.map((project) => (
                <button
                  aria-pressed={selectedProject.id === project.id}
                  key={project.id}
                  onClick={() => handleProjectSelect(project.id)}
                  type="button"
                >
                  <span>{project.type}</span>
                  {project.name}
                </button>
              ))}
            </div>

            <article className="case-study">
              <div className="case-main">
                <div className="project-preview" aria-label={uiText.projectPreviewLabel}>
                  <div className="preview-toolbar">
                    <span>{selectedProject.preview.label}</span>
                    <small>{selectedProject.metric}</small>
                  </div>
                  <div className="preview-screen">
                    <strong>{selectedProject.preview.title}</strong>
                    <div>
                      {selectedProject.preview.stats.map((item) => (
                        <span key={item}>{item}</span>
                      ))}
                    </div>
                  </div>
                </div>
                <p className="eyebrow">{selectedProject.type}</p>
                <h3>{selectedProject.name}</h3>
                <p>{selectedProject.impact}</p>
                <div className="case-meta">
                  <div>
                    <span>{uiText.roleLabel}</span>
                    <strong>{selectedProject.role}</strong>
                  </div>
                  <div>
                    <span>{uiText.durationLabel}</span>
                    <strong>{selectedProject.duration}</strong>
                  </div>
                </div>
                <div className="project-notes" id="project-notes">
                  <article>
                    <span>{uiText.challengeLabel}</span>
                    <p>{selectedProject.challenge}</p>
                  </article>
                  <article>
                    <span>{uiText.solutionLabel}</span>
                    <p>{selectedProject.solution}</p>
                  </article>
                  <article>
                    <span>{uiText.highlightsLabel}</span>
                    <ul>
                      {selectedProject.highlights.map((item) => (
                        <li key={item}>{item}</li>
                      ))}
                    </ul>
                  </article>
                </div>
              </div>
              <aside className="case-side">
                <span>{selectedProject.metric}</span>
                <div className="stack-list">
                  {selectedProject.stack.slice(0, 6).map((item) => (
                    <small key={item}>{item}</small>
                  ))}
                </div>
                {selectedProject.repoUrl && (
                  <a
                    className="case-link"
                    href={selectedProject.repoUrl}
                    target="_blank"
                    rel="noreferrer"
                  >
                    {uiText.viewRepository}
                  </a>
                )}
                {selectedProject.scope && (
                  <div className="scope-list">
                    <p>{uiText.scopeLabel}</p>
                    {selectedProject.scope.map((item) => (
                      <small key={item}>{item}</small>
                    ))}
                  </div>
                )}
                <div className="deliverable-list">
                  <p>{uiText.deliverablesLabel}</p>
                  {selectedProject.deliverables.slice(0, 5).map((item) => (
                    <small key={item}>{item}</small>
                  ))}
                </div>
              </aside>
            </article>
          </section>

          <section className="section-block" id="experience" aria-labelledby="experience-heading">
            <div className="section-heading">
              <p>{uiText.experienceEyebrow}</p>
              <h2 id="experience-heading">{uiText.experienceTitle}</h2>
            </div>
            <div className="timeline">
              {experience.map((item) => (
                <article className="timeline-item" key={item.role}>
                  <div className="timeline-meta">
                    <span>{item.period}</span>
                    <small>{item.stack}</small>
                  </div>
                  <div>
                    <h3>{item.role}</h3>
                    <p className="company">{item.company}</p>
                    <ul>
                      {item.bullets.map((bullet) => (
                        <li key={bullet}>{bullet}</li>
                      ))}
                    </ul>
                  </div>
                </article>
              ))}
            </div>
          </section>

          <section className="section-block education-section" aria-labelledby="education-heading">
            <div className="section-heading split-heading">
              <div>
                <p>{uiText.educationEyebrow}</p>
                <h2 id="education-heading">{uiText.educationTitle}</h2>
              </div>
              <span>{uiText.educationSummary}</span>
            </div>
            <div className="education-grid">
              {credentials.map((item) => (
                <article
                  className={`education-card education-card--${item.status.toLowerCase()}`}
                  key={item.title}
                >
                  <div className="education-card-header">
                    <span className="education-status">{item.status}</span>
                    {item.period && <small>{item.period}</small>}
                  </div>
                  <h3>{item.title}</h3>
                  <p>{item.detail}</p>
                  <small>{item.focus}</small>
                  <span className="education-accent" aria-hidden="true" />
                </article>
              ))}
            </div>
          </section>

          <section
            className="section-block workflow-section"
            aria-labelledby="workflow-heading"
          >
            <div className="section-heading split-heading">
              <div>
                <p>{uiText.workflowEyebrow}</p>
                <h2 id="workflow-heading">{uiText.workflowTitle}</h2>
              </div>
              <span>{uiText.workflowSummary}</span>
            </div>
            <ol className="process-list">
              {workflow.map((step) => (
                <li key={step.label}>
                  <strong>{step.label}</strong>
                  <span>{step.text}</span>
                </li>
              ))}
            </ol>
          </section>

          <section className="section-block collaboration-section" aria-labelledby="collaboration-heading">
            <div className="section-heading">
              <p>{uiText.collaborationEyebrow}</p>
              <h2 id="collaboration-heading">{uiText.collaborationTitle}</h2>
            </div>
            <ul className="collaboration-list">
              {collaboration.map((item) => (
                <li key={item}>{item}</li>
              ))}
            </ul>
          </section>

        </div>

        <aside className="side-column">
          <section className="section-block compact" id="details" aria-labelledby="details-heading">
            <div className="section-heading">
              <p>{uiText.detailsEyebrow}</p>
              <h2 id="details-heading">{uiText.detailsTitle}</h2>
            </div>
            <div className="detail-list">
              {professionalDetails.map((detail) => (
                <article key={detail.label}>
                  <span>{detail.label}</span>
                  <p>{detail.value}</p>
                </article>
              ))}
            </div>
          </section>

          <section className="section-block compact" id="skills" aria-labelledby="skills-heading">
            <div className="section-heading">
              <p>{uiText.toolkitEyebrow}</p>
              <h2 id="skills-heading">{uiText.skillsTitle}</h2>
            </div>
            <div className="skill-groups">
              {skillGroups.map((group) => (
                <article key={group.title}>
                  <div className="skill-title">
                    <h3>{group.title}</h3>
                    <span>{group.score}%</span>
                  </div>
                  <div
                    className="skill-meter"
                    aria-label={`${group.title} ${uiText.proficiencyLabel} ${group.score} ${uiText.percentLabel}`}
                  >
                    <span style={{ width: `${group.score}%` }} />
                  </div>
                  <div className="skill-list">
                    {group.items.map((skill) => (
                      <span key={skill}>{skill}</span>
                    ))}
                  </div>
                </article>
              ))}
            </div>
          </section>

          <section
            className="section-block compact stack-matrix-section"
            id="stack"
            aria-labelledby="stack-heading"
          >
            <div className="section-heading">
              <p>{uiText.stackMatrixEyebrow}</p>
              <h2 id="stack-heading">{uiText.stackTitle}</h2>
            </div>
            <div className="stack-matrix">
              {stackMatrix.map((group) => (
                <article key={group.area}>
                  <h3>{group.area}</h3>
                  <div>
                    {group.tools.map((tool) => (
                      <span key={tool}>{tool}</span>
                    ))}
                  </div>
                </article>
              ))}
            </div>
          </section>

          <section
            className="section-block compact standards-section"
            id="standards"
            aria-labelledby="standards-heading"
          >
            <div className="section-heading">
              <p>{uiText.standardsEyebrow}</p>
              <h2 id="standards-heading">{uiText.standardsTitle}</h2>
            </div>
            <div className="standards-grid">
              {engineeringStandards.map((standard) => (
                <article className="standard-card" key={standard.title}>
                  <span>{standard.proof}</span>
                  <h3>{standard.title}</h3>
                  <p>{standard.detail}</p>
                </article>
              ))}
            </div>
          </section>

          <section className="section-block compact" id="fit" aria-labelledby="fit-heading">
            <div className="section-heading">
              <p>{uiText.roleFitEyebrow}</p>
              <h2 id="fit-heading">{uiText.roleFitTitle}</h2>
            </div>
            <div className="fit-list">
              {roleFit.map((item) => (
                <article key={item.title}>
                  <h3>{item.title}</h3>
                  <p>{item.text}</p>
                </article>
              ))}
            </div>
          </section>

          <section className="section-block compact" aria-labelledby="tools-heading">
            <div className="section-heading">
              <p>{uiText.toolsEyebrow}</p>
              <h2 id="tools-heading">{uiText.toolsTitle}</h2>
            </div>
            <div className="tool-list">
              {tools.map((tool) => (
                <span key={tool}>{tool}</span>
              ))}
            </div>
          </section>

          <section className="section-block compact" aria-labelledby="languages-heading">
            <div className="section-heading">
              <p>{uiText.communicationEyebrow}</p>
              <h2 id="languages-heading">{uiText.languagesTitle}</h2>
            </div>
            <div className="stacked-list">
              {languages.map((language) => (
                <article key={language.title}>
                  <h3>{language.title}</h3>
                  <p>{language.text}</p>
                </article>
              ))}
            </div>
          </section>
        </aside>
      </section>

      <footer className="contact-footer" id="contact">
        <div>
          <p className="eyebrow">{uiText.contactEyebrow}</p>
          <h2>{uiText.contactTitle}</h2>
        </div>
        <div className="contact-methods" aria-label={uiText.contactMethodsLabel}>
          {contactMethods.map((method) => (
            <a
              href={method.href}
              key={method.label}
              target={method.href.startsWith('http') ? '_blank' : undefined}
              rel={method.href.startsWith('http') ? 'noreferrer' : undefined}
            >
              <span>{method.label}</span>
              <strong>{method.value}</strong>
            </a>
          ))}
        </div>
      </footer>
    </main>
  )
}

export default App

import { useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'

const identityBase = {
  name: 'Corniță Cristian',
  email: 'cristi.cornita@gmail.com',
  phone: '+373 60 000 000',
  github: 'https://github.com/CristyMD4',
  linkedin: 'https://linkedin.com/',
}

const languageOptions = [
  { code: 'en', label: 'EN' },
  { code: 'ro', label: 'RO' },
  { code: 'ru', label: 'RU' },
]

const projectRepoUrls = {
  mrstweb: 'https://github.com/CristyMD4/MRSTWeb',
}

const credentialVariants = ['student', 'specialization', 'certificate', 'certificate', 'internship', 'practice']

function translatedArray(t, key) {
  const value = t(key, { returnObjects: true })
  return Array.isArray(value) ? value : []
}

function App() {
  const { t, i18n } = useTranslation()
  const identity = useMemo(
    () => ({ ...identityBase, ...t('identity', { returnObjects: true }) }),
    [t],
  )
  const metrics = translatedArray(t, 'metrics')
  const skillGroups = translatedArray(t, 'skillGroups')
  const achievements = translatedArray(t, 'achievements')
  const strengths = translatedArray(t, 'strengths')
  const roleFit = translatedArray(t, 'roleFit')
  const roleSignals = translatedArray(t, 'roleSignals')
  const engineeringStandards = translatedArray(t, 'engineeringStandards')
  const stackMatrix = translatedArray(t, 'stackMatrix')
  const projects = useMemo(
    () =>
      translatedArray(t, 'projects').map((project) => ({
        ...project,
        repoUrl: projectRepoUrls[project.id],
      })),
    [t],
  )
  const experience = translatedArray(t, 'experience')
  const workflow = translatedArray(t, 'workflow')
  const tools = translatedArray(t, 'tools')
  const credentials = translatedArray(t, 'credentials')
  const collaboration = translatedArray(t, 'collaboration')
  const languageLevels = translatedArray(t, 'languages')
  const professionalDetails = useMemo(
    () => [
      { label: t('professionalDetails.location'), value: identity.location },
      { label: t('professionalDetails.availability'), value: identity.availability },
      { label: t('professionalDetails.workMode'), value: identity.workMode },
      { label: t('professionalDetails.focus'), value: t('professionalDetails.focusValue') },
    ],
    [identity.availability, identity.location, identity.workMode, t],
  )
  const contactMethods = useMemo(
    () => [
      {
        label: t('ui.labels.email'),
        value: identity.email,
        href: `mailto:${identity.email}`,
      },
      {
        label: t('ui.labels.github'),
        value: 'github.com/CristyMD4',
        href: identity.github,
      },
      {
        label: t('ui.labels.location'),
        value: identity.location,
        href: '#details',
      },
    ],
    [identity.email, identity.github, identity.location, t],
  )
  const summaryStrip = translatedArray(t, 'summaryStrip')
  const [selectedProjectId, setSelectedProjectId] = useState('mrstweb')
  const [isHeaderHidden, setIsHeaderHidden] = useState(false)
  const lastScrollY = useRef(0)
  const ticking = useRef(false)
  const selectedProject = useMemo(
    () => projects.find((project) => project.id === selectedProjectId),
    [selectedProjectId],
  )

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

  return (
    <main className="cv-page">
      <header
        aria-hidden={isHeaderHidden}
        aria-label={t('ui.aria.navigation')}
        className={`topbar${isHeaderHidden ? ' topbar--hidden' : ''}`}
        inert={isHeaderHidden ? true : undefined}
      >
        <a className="brand" href="#profile">
          {identity.name}
        </a>
        <nav>
          <a href="#projects">{t('ui.nav.projects')}</a>
          <a href="#experience">{t('ui.nav.experience')}</a>
          <a href="#skills">{t('ui.nav.skills')}</a>
          <a href="#stack">{t('ui.nav.stack')}</a>
          <a href="#standards">{t('ui.nav.standards')}</a>
          <a href="#impact">{t('ui.nav.impact')}</a>
          <a href="#fit">{t('ui.nav.fit')}</a>
          <a href="#details">{t('ui.nav.details')}</a>
          <a href="#contact">{t('ui.nav.contact')}</a>
        </nav>
        <div className="topbar-actions">
          <div className="language-switcher" aria-label={t('ui.aria.languageSwitcher')}>
            {languageOptions.map((language) => (
              <button
                aria-pressed={i18n.resolvedLanguage === language.code}
                key={language.code}
                onClick={() => i18n.changeLanguage(language.code)}
                type="button"
              >
                {language.label}
              </button>
            ))}
          </div>
          <button type="button" onClick={() => window.print()}>
            {t('ui.actions.downloadPdf')}
          </button>
          <button type="button" onClick={() => window.print()}>
            {t('ui.actions.printCv')}
          </button>
        </div>
      </header>

      <section className="hero-section" id="profile" aria-labelledby="candidate-name">
        <div className="hero-copy">
          <div className="summary-strip" aria-label={t('ui.aria.highlights')}>
            {summaryStrip.map((item) => (
              <span key={item}>{item}</span>
            ))}
            <span>{identity.location}</span>
          </div>
          <p className="eyebrow">{t('ui.sections.profileEyebrow')}</p>
          <h1 id="candidate-name">{identity.name}</h1>
          <p className="role">{identity.role}</p>
          <p className="intro">{identity.summary}</p>
          <div className="hero-actions" aria-label={t('ui.aria.contactLinks')}>
            <a href={`mailto:${identity.email}`}>{identity.email}</a>
            <a href={`tel:${identity.phone.replaceAll(' ', '')}`}>{identity.phone}</a>
            <a href={identity.github} target="_blank" rel="noreferrer">
              GitHub
            </a>
            <a href={identity.linkedin} target="_blank" rel="noreferrer">
              LinkedIn
            </a>
          </div>
          <div className="role-signal-grid" aria-label={t('ui.aria.roleFocus')}>
            {roleSignals.map((signal) => (
              <article key={signal.title}>
                <h2>{signal.title}</h2>
                <p>{signal.text}</p>
              </article>
            ))}
          </div>
        </div>

        <aside className="profile-panel" aria-label={t('ui.aria.careerSnapshot')}>
          <div className="panel-header">
            <span>{t('ui.labels.developerProfile')}</span>
            <strong>{t('ui.labels.open')}</strong>
          </div>
          <div className="avatar-card" aria-label={t('ui.aria.avatar')}>
            <div className="avatar-mark" aria-hidden="true">
              CC
            </div>
            <div>
              <h2>{identity.name}</h2>
              <p>{identity.role}</p>
            </div>
          </div>
          <div className="profile-summary">
            <p>{identity.profileSummary}</p>
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

      <section className="systems-row" aria-label={t('ui.aria.capabilityOverview')}>
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
                <p>{t('ui.sections.impactEyebrow')}</p>
                <h2 id="impact-heading">{t('ui.sections.impactTitle')}</h2>
              </div>
              <span>{t('ui.sections.impactSubtitle')}</span>
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
                <p>{t('ui.sections.strengthsEyebrow')}</p>
                <h2 id="strengths-heading">{t('ui.sections.strengthsTitle')}</h2>
              </div>
              <span>{t('ui.sections.strengthsSubtitle')}</span>
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
                <p>{t('ui.sections.projectsEyebrow')}</p>
                <h2 id="projects-heading">{t('ui.sections.projectsTitle')}</h2>
              </div>
              <span>{t('ui.sections.projectsSubtitle')}</span>
            </div>

            <div className="project-switcher" role="group" aria-label={t('ui.aria.projectCases')}>
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
                <div className="project-preview" aria-label={t('ui.aria.projectPreview')}>
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
                    <span>{t('ui.labels.role')}</span>
                    <strong>{selectedProject.role}</strong>
                  </div>
                  <div>
                    <span>{t('ui.labels.duration')}</span>
                    <strong>{selectedProject.duration}</strong>
                  </div>
                </div>
                <div className="project-notes" id="project-notes">
                  <article>
                    <span>{t('ui.labels.challenge')}</span>
                    <p>{selectedProject.challenge}</p>
                  </article>
                  <article>
                    <span>{t('ui.labels.solution')}</span>
                    <p>{selectedProject.solution}</p>
                  </article>
                  <article>
                    <span>{t('ui.labels.highlights')}</span>
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
                  <p>{t('ui.labels.techStack')}</p>
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
                    {t('ui.actions.viewRepository')}
                  </a>
                )}
                {selectedProject.scope && (
                  <div className="scope-list">
                    <p>{t('ui.labels.scope')}</p>
                    {selectedProject.scope.map((item) => (
                      <small key={item}>{item}</small>
                    ))}
                  </div>
                )}
                <div className="deliverable-list">
                  <p>{t('ui.labels.deliverables')}</p>
                  {selectedProject.deliverables.slice(0, 5).map((item) => (
                    <small key={item}>{item}</small>
                  ))}
                </div>
              </aside>
            </article>
          </section>

          <section className="section-block" id="experience" aria-labelledby="experience-heading">
            <div className="section-heading">
              <p>{t('ui.sections.experienceEyebrow')}</p>
              <h2 id="experience-heading">{t('ui.sections.experienceTitle')}</h2>
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
                <p>{t('ui.sections.educationEyebrow')}</p>
                <h2 id="education-heading">{t('ui.sections.educationTitle')}</h2>
              </div>
              <span>{t('ui.sections.educationSubtitle')}</span>
            </div>
            <div className="education-grid">
              {credentials.map((item, index) => (
                <article
                  className={`education-card education-card--${credentialVariants[index]}`}
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
                <p>{t('ui.sections.workflowEyebrow')}</p>
                <h2 id="workflow-heading">{t('ui.sections.workflowTitle')}</h2>
              </div>
              <span>{t('ui.sections.workflowSubtitle')}</span>
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
              <p>{t('ui.sections.collaborationEyebrow')}</p>
              <h2 id="collaboration-heading">{t('ui.sections.collaborationTitle')}</h2>
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
              <p>{t('ui.sections.detailsEyebrow')}</p>
              <h2 id="details-heading">{t('ui.sections.detailsTitle')}</h2>
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
              <p>{t('ui.sections.skillsEyebrow')}</p>
              <h2 id="skills-heading">{t('ui.sections.skillsTitle')}</h2>
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
                    aria-label={t('ui.aria.proficiency', {
                      title: group.title,
                      score: group.score,
                    })}
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
              <p>{t('ui.sections.stackEyebrow')}</p>
              <h2 id="stack-heading">{t('ui.sections.stackTitle')}</h2>
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
              <p>{t('ui.sections.standardsEyebrow')}</p>
              <h2 id="standards-heading">{t('ui.sections.standardsTitle')}</h2>
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
              <p>{t('ui.sections.fitEyebrow')}</p>
              <h2 id="fit-heading">{t('ui.sections.fitTitle')}</h2>
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
              <p>{t('ui.sections.toolsEyebrow')}</p>
              <h2 id="tools-heading">{t('ui.sections.toolsTitle')}</h2>
            </div>
            <div className="tool-list">
              {tools.map((tool) => (
                <span key={tool}>{tool}</span>
              ))}
            </div>
          </section>

          <section className="section-block compact" aria-labelledby="languages-heading">
            <div className="section-heading">
              <p>{t('ui.sections.languagesEyebrow')}</p>
              <h2 id="languages-heading">{t('ui.sections.languagesTitle')}</h2>
            </div>
            <div className="stacked-list">
              {languageLevels.map((language) => (
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
          <p className="eyebrow">{t('ui.sections.contactEyebrow')}</p>
          <h2>{t('ui.sections.contactTitle')}</h2>
        </div>
        <div className="contact-methods" aria-label={t('ui.aria.contactMethods')}>
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

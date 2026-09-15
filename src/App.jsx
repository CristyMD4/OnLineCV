import { useEffect, useMemo, useRef, useState } from 'react'

const identity = {
  name: 'Cornita Cristian',
  role: 'Front End Developer',
  email: 'cristian.cornita@gmail.com',
  phone: '+373 60 000 000',
  location: 'Chisinau, Moldova',
  github: 'https://github.com/CristyMD4',
  linkedin: 'https://linkedin.com/',
  availability: 'Available for junior front-end roles',
  workMode: 'Remote, hybrid, or on-site collaboration',
  summary:
    'Front-end developer focused on React product interfaces, accessible user flows, and maintainable UI systems for teams that value polish, speed, and reliable delivery.',
}

const metrics = [
  { value: '1+', label: 'years in front-end development' },
  { value: '35+', label: 'interfaces, pages, and flows delivered' },
  { value: '92', label: 'performance score target' },
  { value: '8', label: 'component systems improved' },
]

const skillGroups = [
  {
    title: 'Interface Architecture',
    score: 92,
    items: ['React', 'Component Design', 'State Patterns', 'Routing', 'Reusable APIs'],
  },
  {
    title: 'Visual Implementation',
    score: 88,
    items: ['CSS Grid', 'Responsive Layouts', 'Tailwind CSS', 'Design Tokens', 'Motion Polish'],
  },
  {
    title: 'Product Quality',
    score: 84,
    items: ['Accessibility', 'Performance', 'QA Support', 'Code Review', 'Documentation'],
  },
  {
    title: 'Integration',
    score: 79,
    items: ['REST APIs', 'Forms', 'Validation', 'Auth Flows', 'Git Workflow'],
  },
]

const achievements = [
  {
    title: 'Performance Recovery',
    detail:
      'Reduced landing page load time by 38% using image optimization, leaner component loading, and CSS cleanup.',
  },
  {
    title: 'Accessible Forms',
    detail:
      'Built reusable form patterns with keyboard navigation, validation states, and consistent error messaging.',
  },
  {
    title: 'Design System Handoff',
    detail:
      'Created component documentation that shortened handoff time between design and engineering.',
  },
  {
    title: 'Mobile Conversion Flow',
    detail:
      'Improved mobile checkout clarity by simplifying steps, visual hierarchy, and interaction feedback.',
  },
]

const strengths = [
  {
    title: 'UI precision',
    text: 'Comfortable matching design intent closely, including spacing rhythm, typography, responsive states, and visual hierarchy.',
  },
  {
    title: 'Maintainable code',
    text: 'Prefers small reusable components, clear naming, predictable state, and styles that another developer can safely extend.',
  },
  {
    title: 'User-centered decisions',
    text: 'Looks beyond the screen layout to reduce confusion, improve flows, and make interface states easier to understand.',
  },
]

const roleFit = [
  {
    title: 'Best Fit',
    text: 'React front-end roles where product quality, responsive UI, and maintainable components matter.',
  },
  {
    title: 'Strongest Value',
    text: 'Turning designs and requirements into clear interfaces with realistic states and careful polish.',
  },
  {
    title: 'Team Impact',
    text: 'Easy handoff, steady communication, clean code habits, and willingness to improve existing screens.',
  },
]

const roleSignals = [
  {
    title: 'Product UI Engineering',
    text: 'Turns requirements into responsive interfaces with clear states, predictable behavior, and thoughtful content structure.',
  },
  {
    title: 'Front-End Systems',
    text: 'Builds reusable components, design-token habits, and layout rules that keep screens consistent as products grow.',
  },
  {
    title: 'Quality Ownership',
    text: 'Checks accessibility, performance, browser behavior, and interaction details before handoff.',
  },
]

const engineeringStandards = [
  {
    title: 'Accessible by default',
    detail: 'Semantic HTML, keyboard paths, visible focus states, readable contrast, and practical form feedback.',
    proof: 'WCAG-aware UI',
  },
  {
    title: 'Performance-minded builds',
    detail: 'Lean component structure, careful asset loading, CSS cleanup, and Lighthouse-oriented review.',
    proof: 'Fast interfaces',
  },
  {
    title: 'Component discipline',
    detail: 'Reusable layout primitives, clear prop boundaries, naming conventions, and documented UI states.',
    proof: 'Reusable patterns',
  },
  {
    title: 'Responsive QA',
    detail: 'Mobile spacing, desktop density, browser differences, loading states, and empty states checked early.',
    proof: 'Reliable screens',
  },
]

const stackMatrix = [
  {
    area: 'Frontend',
    tools: ['React', 'Vite', 'JavaScript', 'TypeScript', 'React Router'],
  },
  {
    area: 'Styling',
    tools: ['CSS Grid', 'Responsive CSS', 'Tailwind CSS', 'Design Tokens'],
  },
  {
    area: 'Forms & Data',
    tools: ['React Hook Form', 'Zod', 'REST APIs', 'Validation States'],
  },
  {
    area: 'Quality',
    tools: ['Accessibility', 'Lighthouse', 'Git Workflow', 'Browser QA'],
  },
]

const projects = [
  {
    id: 'mrstweb',
    name: 'MRSTWeb / LuxWash Platform',
    type: 'Full-stack personal project',
    role: 'Front-end architecture, booking flows, admin UI, and API integration',
    duration: 'Personal project',
    repoUrl: 'https://github.com/CristyMD4/MRSTWeb',
    stack: [
      'React',
      'Vite',
      'React Router',
      'i18next',
      'React Hook Form',
      'Zod',
      'ASP.NET Core',
      'Entity Framework Core',
      'SQL Server',
      'JWT Auth',
    ],
    metric: 'Client + admin + employee flows',
    preview: {
      label: 'Service platform',
      title: 'Booking, shop, and dashboard flows',
      stats: ['4 user areas', 'JWT access', 'SQL data model'],
    },
    scope: [
      'Public service pages',
      'Booking flow',
      'Shop and cart',
      'Client account',
      'Admin dashboard',
      'Employee dashboard',
    ],
    challenge:
      'A service business platform needed public pages, customer booking, product shopping, and operational dashboards in one coherent web experience.',
    solution:
      'Built a routed React front end with multilingual content, booking and account areas, shop/cart pages, admin management screens, and employee dashboard views backed by a structured .NET API.',
    impact:
      'The project demonstrates end-to-end product thinking across customer experience, internal operations, authentication, data models, and maintainable UI architecture.',
    deliverables: [
      'Booking flow',
      'Service catalog',
      'Shop and cart',
      'Client account area',
      'Admin dashboard',
      'Employee dashboard',
      'JWT-protected API',
      'SQL Server data model',
    ],
    highlights: [
      'Built public, client, admin, and employee flows inside one routed React application.',
      'Used React Hook Form, Zod, and JWT-aware screens for cleaner validation and protected user journeys.',
      'Connected the interface to an ASP.NET Core, Entity Framework, and SQL Server back-end structure.',
    ],
  },
  {
    id: 'analytics',
    name: 'Portfolio Dashboard',
    type: 'React analytics interface',
    role: 'Front-end implementation and UI architecture',
    duration: '4 weeks',
    stack: ['React', 'Vite', 'CSS Grid', 'REST data'],
    metric: '42% faster filtering',
    preview: {
      label: 'Analytics UI',
      title: 'KPI scanning and filter workspace',
      stats: ['KPI cards', 'Fast filters', 'Loading states'],
    },
    challenge:
      'A dashboard needed dense data views without becoming visually heavy or slow on mid-range laptops.',
    solution:
      'Created reusable summary panels, responsive table regions, and lightweight client-side filtering patterns.',
    impact:
      'Users could scan KPIs, compare states, and move between filtered views with fewer layout shifts.',
    deliverables: ['KPI cards', 'Responsive table layout', 'Filter state model', 'Empty and loading states'],
    highlights: [
      'Designed KPI cards, filter controls, and responsive data regions for quick scanning.',
      'Kept filtering lightweight on the client to improve perceived speed.',
      'Added empty and loading states so the dashboard felt complete in realistic data conditions.',
    ],
  },
  {
    id: 'commerce',
    name: 'E-commerce Interface',
    type: 'Product purchase flow',
    role: 'Mobile-first interface build',
    duration: '5 weeks',
    stack: ['React', 'JavaScript', 'Tailwind CSS', 'Accessible Forms'],
    metric: '31% fewer checkout steps',
    preview: {
      label: 'Commerce flow',
      title: 'Mobile-first product and checkout path',
      stats: ['Cart states', 'Form feedback', 'Touch layout'],
    },
    challenge:
      'The shopping flow had inconsistent mobile states and too many unclear transition points.',
    solution:
      'Rebuilt product cards, cart states, and checkout steps around simpler hierarchy and persistent feedback.',
    impact:
      'Customers had a clearer path from browsing to purchase, especially on small screens.',
    deliverables: ['Product grid', 'Cart summary', 'Checkout states', 'Form validation patterns'],
    highlights: [
      'Reworked product browsing, cart review, and checkout steps around mobile-first use.',
      'Improved form labels, validation messages, and touch target spacing.',
      'Kept order details visible so customers could move through checkout with less confusion.',
    ],
  },
  {
    id: 'system',
    name: 'Design System Starter',
    type: 'Component library foundation',
    role: 'Component documentation and UI standards',
    duration: '3 weeks',
    stack: ['React', 'CSS Tokens', 'Documentation', 'UI Governance'],
    metric: '6 reusable patterns',
    preview: {
      label: 'UI system',
      title: 'Reusable components and state rules',
      stats: ['Tokens', 'Forms', 'Usage notes'],
    },
    challenge:
      'Teams were recreating similar buttons, forms, and spacing rules across project screens.',
    solution:
      'Defined shared tokens, component rules, usage notes, and examples for the most common UI patterns.',
    impact:
      'New screens became easier to ship consistently, with fewer visual regressions during review.',
    deliverables: ['Button rules', 'Form examples', 'Spacing tokens', 'Usage documentation'],
    highlights: [
      'Defined reusable rules for buttons, forms, spacing, and common UI states.',
      'Created small documentation examples for easier design-to-development handoff.',
      'Reduced repeated styling decisions by turning common patterns into shared standards.',
    ],
  },
]

const experience = [
  {
    role: 'Front End Developer',
    company: 'Freelance / Contract',
    period: '2026 - Present',
    stack: 'React, TypeScript, Tailwind CSS, Vite',
    bullets: [
      'Build responsive websites and web apps with React, modern CSS, and component-driven architecture.',
      'Translate Figma layouts into polished interfaces with careful spacing, typography, and state handling.',
      'Improve page speed, accessibility, and maintainability across existing front-end codebases.',
      'Partner with clients to define scope, estimate delivery, and turn feedback into focused interface updates.',
    ],
  },
  {
    role: 'Internship Front End Developer',
    company: 'BSW TECH',
    period: '2025 - 2026',
    stack: 'HTML, CSS, JavaScript, CMS templates',
    bullets: [
      'Shipped landing pages, admin screens, and reusable UI sections for client projects.',
      'Collaborated with designers and back-end developers to integrate APIs and refine user flows.',
      'Maintained clean Git workflows and reviewed UI changes before release.',
      'Supported QA by fixing layout issues, browser inconsistencies, and accessibility defects.',
    ],
  },
]

const workflow = [
  {
    label: 'Discover',
    text: 'Clarify user goals, content priorities, edge cases, and technical constraints.',
  },
  {
    label: 'Systemize',
    text: 'Define layout rules, reusable components, states, and responsive behavior.',
  },
  {
    label: 'Build',
    text: 'Implement semantic React views with accessible controls and resilient CSS.',
  },
  {
    label: 'Refine',
    text: 'Review performance, polish interactions, test layouts, and prepare handoff.',
  },
]

const tools = [
  'VS Code',
  'Figma',
  'Chrome DevTools',
  'GitHub',
  'npm',
  'Vercel-style deployments',
  'Lighthouse',
  'Postman',
]

const professionalDetails = [
  { label: 'Location', value: identity.location },
  { label: 'Availability', value: identity.availability },
  { label: 'Work mode', value: identity.workMode },
  { label: 'Focus', value: 'React product interfaces, responsive websites, and UI systems' },
]

const credentials = [
  {
    title: 'Student of Tehnical University of Moldova',
    detail: 'Faculty of Computers, Informatics and Microelectronics, 2024 - 2028',
    focus: 'Programming fundamentals, databases, software design, and web technologies.',
    status: 'Student',
  },
  {
    title: 'Front-End Engineering',
    detail: 'Advanced React, accessibility, testing, and performance.',
    focus: 'Component architecture, routing patterns, state handling, and production UI quality.',
    status: 'Specialization',
  },
  {
    title: 'Front-End Development Certificate',
    detail: 'STEP IT Academy',
    focus: 'Front-end fundamentals, responsive layouts, JavaScript, React practice, and project-based UI development.',
    status: 'Certificate',
  },
  {
    title: 'English B2 Certificate',
    detail: 'Friendly School',
    focus: 'Upper-intermediate English communication for technical collaboration, documentation, and team discussions.',
    status: 'Certificate',
  },
  {
    title: 'Professional Development',
    detail: 'Design systems, API integration, and modern front-end tooling.',
    focus: 'Design tokens, API-connected interfaces, Git workflow, and documentation habits.',
    status: 'Internship',
  },
  {
    title: 'Accessibility Practice',
    detail: 'Semantic HTML, keyboard flows, contrast checks, and form feedback.',
    focus: 'Practical accessibility reviews for forms, navigation, focus states, and responsive layouts.',
    status: 'Practice',
  },
]

const collaboration = [
  'Clear progress updates and practical tradeoff notes during implementation.',
  'Comfortable working from Figma files, written specs, user stories, or existing code.',
  'Careful with details that usually cause friction: loading states, empty states, mobile spacing, and browser differences.',
]

const contactMethods = [
  {
    label: 'Email',
    value: identity.email,
    href: `mailto:${identity.email}`,
  },
  {
    label: 'GitHub',
    value: 'github.com/CristyMD4',
    href: identity.github,
  },
  {
    label: 'Location',
    value: identity.location,
    href: '#details',
  },
]

function App() {
  const [selectedProjectId, setSelectedProjectId] = useState(projects[0].id)
  const [copyStatus, setCopyStatus] = useState('')
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

  async function handleCopyEmail() {
    try {
      await navigator.clipboard.writeText(identity.email)
      setCopyStatus('Email copied')
    } catch {
      setCopyStatus('Use the email link above')
    }
  }

  return (
    <main className="cv-page">
      <header
        aria-hidden={isHeaderHidden}
        aria-label="Page navigation"
        className={`topbar${isHeaderHidden ? ' topbar--hidden' : ''}`}
        inert={isHeaderHidden ? true : undefined}
      >
        <a className="brand" href="#profile">
          {identity.name}
        </a>
        <nav>
          <a href="#projects">Projects</a>
          <a href="#experience">Experience</a>
          <a href="#skills">Skills</a>
          <a href="#stack">Stack</a>
          <a href="#standards">Standards</a>
          <a href="#impact">Impact</a>
          <a href="#fit">Fit</a>
          <a href="#details">Details</a>
          <a href="#contact">Contact</a>
        </nav>
        <div className="topbar-actions">
          <button type="button" onClick={() => window.print()}>
            Download PDF
          </button>
          <button type="button" onClick={() => window.print()}>
            Print CV
          </button>
        </div>
      </header>

      <section className="hero-section" id="profile" aria-labelledby="candidate-name">
        <div className="hero-copy">
          <div className="summary-strip" aria-label="Professional highlights">
            <span>React Developer</span>
            <span>Accessible UI</span>
            <span>Product Interfaces</span>
            <span>{identity.location}</span>
          </div>
          <p className="eyebrow">Front-end developer</p>
          <h1 id="candidate-name">{identity.name}</h1>
          <p className="role">{identity.role}</p>
          <p className="intro">{identity.summary}</p>
          <div className="hero-actions" aria-label="Contact links">
            <a href={`mailto:${identity.email}`}>{identity.email}</a>
            <a href={`tel:${identity.phone.replaceAll(' ', '')}`}>{identity.phone}</a>
            <a href={identity.github} target="_blank" rel="noreferrer">
              GitHub
            </a>
            <a href={identity.linkedin} target="_blank" rel="noreferrer">
              LinkedIn
            </a>
            <button type="button" onClick={handleCopyEmail}>
              Copy Email
            </button>
          </div>
          <div className="role-signal-grid" aria-label="Role focus">
            {roleSignals.map((signal) => (
              <article key={signal.title}>
                <h2>{signal.title}</h2>
                <p>{signal.text}</p>
              </article>
            ))}
          </div>
          {copyStatus && (
            <p className="copy-status" aria-live="polite">
              {copyStatus}
            </p>
          )}
        </div>

        <aside className="profile-panel" aria-label="Career snapshot">
          <div className="panel-header">
            <span>Developer Profile</span>
            <strong>Open</strong>
          </div>
          <div className="avatar-card" aria-label="Candidate avatar">
            <div className="avatar-mark" aria-hidden="true">
              CC
            </div>
            <div>
              <h2>{identity.name}</h2>
              <p>{identity.role}</p>
            </div>
          </div>
          <div className="profile-summary">
            <p>
              I combine UI craft, product thinking, and practical engineering
              habits to ship interfaces that teams can keep improving.
            </p>
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

      <section className="systems-row" aria-label="Capability overview">
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
                <p>Impact</p>
                <h2 id="impact-heading">Evidence Of Strong Delivery</h2>
              </div>
              <span>Measurable improvements across performance, usability, and consistency.</span>
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
                <p>Strengths</p>
                <h2 id="strengths-heading">How I Create Value</h2>
              </div>
              <span>Practical habits that make projects easier to ship and maintain.</span>
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
                <p>Case Studies</p>
                <h2 id="projects-heading">Project Lab</h2>
              </div>
              <span>Short project summaries with role, stack, key output, and links.</span>
            </div>

            <div className="project-switcher" role="group" aria-label="Project case studies">
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
                <div className="project-preview" aria-label="Project preview">
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
                    <span>Role</span>
                    <strong>{selectedProject.role}</strong>
                  </div>
                  <div>
                    <span>Duration</span>
                    <strong>{selectedProject.duration}</strong>
                  </div>
                </div>
                <div className="project-notes" id="project-notes">
                  <article>
                    <span>Challenge</span>
                    <p>{selectedProject.challenge}</p>
                  </article>
                  <article>
                    <span>Solution</span>
                    <p>{selectedProject.solution}</p>
                  </article>
                  <article>
                    <span>Highlights</span>
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
                    View repository
                  </a>
                )}
                {selectedProject.scope && (
                  <div className="scope-list">
                    <p>Scope</p>
                    {selectedProject.scope.map((item) => (
                      <small key={item}>{item}</small>
                    ))}
                  </div>
                )}
                <div className="deliverable-list">
                  <p>Deliverables</p>
                  {selectedProject.deliverables.slice(0, 5).map((item) => (
                    <small key={item}>{item}</small>
                  ))}
                </div>
              </aside>
            </article>
          </section>

          <section className="section-block" id="experience" aria-labelledby="experience-heading">
            <div className="section-heading">
              <p>Experience</p>
              <h2 id="experience-heading">Selected Work History</h2>
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
                <p>Education</p>
                <h2 id="education-heading">Training And Professional Growth</h2>
              </div>
              <span>Formal study supported by continuous front-end practice.</span>
            </div>
            <div className="education-grid">
              {credentials.map((item) => (
                <article className="education-card" key={item.title}>
                  <span>{item.status}</span>
                  <div>
                    <h3>{item.title}</h3>
                    <p>{item.detail}</p>
                    <small>{item.focus}</small>
                  </div>
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
                <p>Workflow</p>
                <h2 id="workflow-heading">Delivery System</h2>
              </div>
              <span>Clear steps from planning to polished handoff.</span>
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
              <p>Collaboration</p>
              <h2 id="collaboration-heading">Working Style</h2>
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
              <p>Details</p>
              <h2 id="details-heading">Professional Details</h2>
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
              <p>Toolkit</p>
              <h2 id="skills-heading">Skill Architecture</h2>
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
                    aria-label={`${group.title} proficiency ${group.score} percent`}
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
              <p>Stack Matrix</p>
              <h2 id="stack-heading">Technical Coverage</h2>
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
              <p>Engineering Standards</p>
              <h2 id="standards-heading">Production Ready UI</h2>
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
              <p>Role Fit</p>
              <h2 id="fit-heading">What I Bring</h2>
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
              <p>Tools</p>
              <h2 id="tools-heading">Daily Stack</h2>
            </div>
            <div className="tool-list">
              {tools.map((tool) => (
                <span key={tool}>{tool}</span>
              ))}
            </div>
          </section>

          <section className="section-block compact" aria-labelledby="languages-heading">
            <div className="section-heading">
              <p>Communication</p>
              <h2 id="languages-heading">Languages</h2>
            </div>
            <div className="stacked-list">
              <article>
                <h3>English</h3>
                <p>Professional working proficiency</p>
              </article>
              <article>
                <h3>Romanian</h3>
                <p>Native or bilingual proficiency</p>
              </article>
            </div>
          </section>
        </aside>
      </section>

      <footer className="contact-footer" id="contact">
        <div>
          <p className="eyebrow">Contact</p>
          <h2>Available for front-end development roles</h2>
        </div>
        <div className="contact-methods" aria-label="Contact methods">
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

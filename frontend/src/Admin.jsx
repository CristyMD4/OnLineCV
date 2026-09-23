import { useEffect, useMemo, useRef, useState } from 'react'
import { API_BASE_URL } from './api.js'
import './admin.css'

const TOKEN_KEY = 'onlinecv-admin-token'

const SECTION_META = {
  identity: ['Personal profile', 'Name, role, contact details, availability, and introduction.'],
  projects: ['Projects', 'Case studies, project links, outcomes, and deliverables.'],
  experience: ['Experience', 'Roles, employers, dates, technology, and responsibilities.'],
  skillGroups: ['Skills', 'Skill categories, proficiency scores, and individual skills.'],
  credentials: ['Education & credentials', 'Education, certificates, and professional development.'],
  achievements: ['Achievements', 'Highlighted results and supporting details.'],
  metrics: ['Career metrics', 'The headline numbers shown in the career snapshot.'],
  strengths: ['Strengths', 'Professional strengths and supporting descriptions.'],
  engineeringStandards: ['Engineering standards', 'Development principles and proof points.'],
  workflow: ['Workflow', 'The steps used to plan and deliver work.'],
  collaboration: ['Collaboration', 'Statements about communication and teamwork.'],
  professionalDetails: ['Professional details', 'Location, availability, work mode, and focus.'],
  contactMethods: ['Contact methods', 'Contact labels, visible values, and destination links.'],
  languages: ['Languages', 'Languages and communication proficiency.'],
  tools: ['Tools', 'Tools displayed in the toolkit section.'],
  stackMatrix: ['Technology stack', 'Technology areas and the tools used in each one.'],
  roleFit: ['Role fit', 'Reasons this profile fits the target position.'],
  roleSignals: ['Role signals', 'Short supporting signals shown near the profile.'],
  summaryHighlights: ['Summary highlights', 'Short points displayed with the profile summary.'],
  navigation: ['Navigation', 'Menu labels and the sections they open.'],
  uiText: ['Page wording', 'Headings, labels, and supporting copy used across the CV.'],
}

const SECTION_ORDER = Object.keys(SECTION_META)
const LONG_TEXT_FIELDS = new Set([
  'summary', 'text', 'detail', 'focus', 'challenge', 'solution', 'impact', 'pageDescription',
])

const ARRAY_TEMPLATES = {
  achievements: { title: '', detail: '' },
  contactMethods: { label: '', value: '', href: '' },
  credentials: { title: '', detail: '', focus: '', status: '', period: '' },
  engineeringStandards: { title: '', detail: '', proof: '' },
  experience: { role: '', company: '', period: '', stack: '', bullets: [] },
  languages: { title: '', text: '' },
  metrics: { value: '', label: '' },
  navigation: { label: '', href: '' },
  professionalDetails: { label: '', value: '' },
  projects: { id: '', name: '', type: '', role: '', duration: '', repoUrl: '', stack: [], metric: '', preview: '', scope: '', challenge: '', solution: '', impact: '', deliverables: [], highlights: [] },
  roleFit: { title: '', text: '' },
  roleSignals: { title: '', text: '' },
  skillGroups: { title: '', score: 0, items: [] },
  stackMatrix: { area: '', tools: [] },
  strengths: { title: '', text: '' },
  workflow: { label: '', text: '' },
}

function humanize(value) {
  return value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[-_]/g, ' ')
    .replace(/^./, (character) => character.toUpperCase())
}

function cloneEmpty(value) {
  if (Array.isArray(value)) return []
  if (value && typeof value === 'object') {
    return Object.fromEntries(Object.entries(value).map(([key, item]) => [key, cloneEmpty(item)]))
  }
  if (typeof value === 'number') return 0
  if (typeof value === 'boolean') return false
  return ''
}

function updateAtPath(source, path, value) {
  const next = structuredClone(source)
  let cursor = next
  path.slice(0, -1).forEach((part) => { cursor = cursor[part] })
  cursor[path.at(-1)] = value
  return next
}

function removeAtPath(source, path) {
  const next = structuredClone(source)
  let cursor = next
  path.slice(0, -1).forEach((part) => { cursor = cursor[part] })
  cursor.splice(path.at(-1), 1)
  return next
}

function appendAtPath(source, path, value) {
  const next = structuredClone(source)
  let cursor = next
  path.forEach((part) => { cursor = cursor[part] })
  cursor.push(value)
  return next
}

function collectChanges(before, after, path = []) {
  if (JSON.stringify(before) === JSON.stringify(after)) return []

  if (Array.isArray(before) && Array.isArray(after)) {
    const changes = []
    const sharedLength = Math.min(before.length, after.length)
    for (let index = 0; index < sharedLength; index += 1) {
      changes.push(...collectChanges(before[index], after[index], [...path, index]))
    }
    for (let index = sharedLength; index < after.length; index += 1) {
      changes.push({ path: [...path, index], before: undefined, after: after[index] })
    }
    for (let index = sharedLength; index < before.length; index += 1) {
      changes.push({ path: [...path, index], before: before[index], after: undefined })
    }
    return changes
  }

  if (before && after && typeof before === 'object' && typeof after === 'object') {
    const keys = new Set([...Object.keys(before), ...Object.keys(after)])
    return [...keys].flatMap((key) => collectChanges(before[key], after[key], [...path, key]))
  }

  return [{ path, before, after }]
}

function formatPath(path) {
  return path.map((part) => (typeof part === 'number' ? `Item ${part + 1}` : humanize(part))).join(' / ')
}

function formatValue(value) {
  if (value === undefined) return 'Not set'
  if (Array.isArray(value)) return value.length === 0 ? 'Empty list' : `${value.length} item(s)`
  if (value && typeof value === 'object') return 'Complete item'
  if (value === '') return 'Empty'
  return String(value)
}

function itemHeading(item, index) {
  if (typeof item === 'string') return `Item ${index + 1}`
  return item?.name || item?.title || item?.role || item?.label || item?.area || `Item ${index + 1}`
}

function FieldEditor({ fieldKey, value, path, onChange, source, depth = 0 }) {
  const label = humanize(fieldKey)

  if (Array.isArray(value)) {
    const sample = value[0]
    const isObjectList = sample && typeof sample === 'object'
    const addItem = () => onChange(appendAtPath(source, path, cloneEmpty(sample ?? ARRAY_TEMPLATES[fieldKey] ?? '')))
    const list = (
      <div className={isObjectList ? 'admin-item-list' : 'admin-string-list'}>
        {value.map((item, index) => {
          const itemPath = [...path, index]
          if (typeof item !== 'object' || item === null) {
            return (
              <div className="admin-string-row" key={`${fieldKey}-${index}`}>
                <span aria-hidden="true">{index + 1}</span>
                <textarea aria-label={`${label} item ${index + 1}`} onChange={(event) => onChange(updateAtPath(source, itemPath, event.target.value))} rows={2} value={item} />
                <button className="admin-button-danger" onClick={() => onChange(removeAtPath(source, itemPath))} type="button">Remove</button>
              </div>
            )
          }

          return (
            <details className="admin-item" key={`${fieldKey}-${index}`}>
              <summary>
                <div><span>Item {index + 1}</span><h3>{itemHeading(item, index)}</h3></div>
                <b>Edit</b>
              </summary>
              <div className="admin-item-body">
                <div className="admin-item-actions"><button className="admin-button-danger" onClick={() => onChange(removeAtPath(source, itemPath))} type="button">Remove item</button></div>
                <div className="admin-field-grid">
                  {Object.entries(item).map(([key, childValue]) => (
                    <FieldEditor depth={depth + 1} fieldKey={key} key={key} onChange={onChange} path={[...itemPath, key]} source={source} value={childValue} />
                  ))}
                </div>
              </div>
            </details>
          )
        })}
      </div>
    )

    if (depth > 0) {
      return (
        <details className="admin-field-group admin-nested-list">
          <summary><div><h3>{label}</h3><p>{value.length} item{value.length === 1 ? '' : 's'}</p></div><b>Edit list</b></summary>
          <div className="admin-nested-content">
            <button className="admin-button-secondary" onClick={addItem} type="button">Add item</button>
            {value.length === 0 ? <p className="admin-empty">No items yet.</p> : list}
          </div>
        </details>
      )
    }

    return (
      <div className="admin-field-group">
        <div className="admin-field-group-heading">
          <div><h3>{label}</h3><p>{value.length} item{value.length === 1 ? '' : 's'}</p></div>
          <button className="admin-button-secondary" onClick={addItem} type="button">Add item</button>
        </div>
        {value.length === 0 ? <p className="admin-empty">No items yet. Add one to include it on the CV.</p> : list}
      </div>
    )
  }

  if (value && typeof value === 'object') {
    return (
      <div className="admin-field-grid admin-field-grid--object">
        {Object.entries(value).map(([key, childValue]) => (
          <FieldEditor depth={depth + 1} fieldKey={key} key={key} onChange={onChange} path={[...path, key]} source={source} value={childValue} />
        ))}
      </div>
    )
  }

  const useTextarea = typeof value === 'string' && (value.length > 80 || LONG_TEXT_FIELDS.has(fieldKey))
  return (
    <label className={`admin-field ${useTextarea ? 'admin-field--wide' : ''}`}>
      <span>{label}</span>
      {useTextarea ? (
        <textarea onChange={(event) => onChange(updateAtPath(source, path, event.target.value))} rows={4} value={value ?? ''} />
      ) : (
        <input onChange={(event) => onChange(updateAtPath(source, path, typeof value === 'number' ? Number(event.target.value) : event.target.value))} type={typeof value === 'number' ? 'number' : 'text'} value={value ?? ''} />
      )}
    </label>
  )
}

function Admin() {
  const [token, setToken] = useState(() => sessionStorage.getItem(TOKEN_KEY) ?? '')
  const [username, setUsername] = useState('admin')
  const [password, setPassword] = useState('')
  const [content, setContent] = useState(null)
  const [savedContent, setSavedContent] = useState(null)
  const [selectedSection, setSelectedSection] = useState('identity')
  const [sectionFilter, setSectionFilter] = useState('')
  const [editorMode, setEditorMode] = useState('form')
  const [jsonDraft, setJsonDraft] = useState('')
  const [jsonError, setJsonError] = useState('')
  const [status, setStatus] = useState('')
  const [statusKind, setStatusKind] = useState('neutral')
  const [isBusy, setIsBusy] = useState(false)
  const loadContentRef = useRef(null)

  const changes = useMemo(() => (content && savedContent ? collectChanges(savedContent, content) : []), [content, savedContent])
  const availableSections = useMemo(() => {
    if (!content) return []
    const keys = [...SECTION_ORDER.filter((key) => key in content), ...Object.keys(content).filter((key) => !SECTION_ORDER.includes(key))]
    return keys.filter((key) => (SECTION_META[key]?.[0] ?? humanize(key)).toLowerCase().includes(sectionFilter.toLowerCase()))
  }, [content, sectionFilter])

  useEffect(() => {
    document.title = 'OnlineCV Admin'
    if (token) loadContentRef.current(true)
  }, [token])

  useEffect(() => {
    const preventAccidentalExit = (event) => {
      if (changes.length === 0) return
      event.preventDefault()
      event.returnValue = ''
    }
    window.addEventListener('beforeunload', preventAccidentalExit)
    return () => window.removeEventListener('beforeunload', preventAccidentalExit)
  }, [changes.length])

  async function loadContent(force = false) {
    if (!force && changes.length > 0 && !window.confirm('Discard your unsaved changes and reload?')) return
    setIsBusy(true)
    setStatus('Loading content...')
    setStatusKind('neutral')

    try {
      const response = await fetch(`${API_BASE_URL}/api/cv`, { cache: 'no-store' })
      if (!response.ok) throw new Error(`Content request failed with ${response.status}`)
      const result = await response.json()
      setContent(result)
      setSavedContent(structuredClone(result))
      setJsonDraft(JSON.stringify(result, null, 2))
      setJsonError('')
      setStatus('Content loaded')
      setStatusKind('success')
    } catch (error) {
      setStatus(error.message)
      setStatusKind('error')
    } finally {
      setIsBusy(false)
    }
  }

  loadContentRef.current = loadContent

  async function handleLogin(event) {
    event.preventDefault()
    setIsBusy(true)
    setStatus('Signing in...')
    setStatusKind('neutral')
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ username, password }),
      })
      if (!response.ok) throw new Error('Invalid username or password')
      const result = await response.json()
      sessionStorage.setItem(TOKEN_KEY, result.token)
      setToken(result.token)
      setPassword('')
      setStatus('Signed in')
      setStatusKind('success')
    } catch (error) {
      setStatus(error.message)
      setStatusKind('error')
    } finally {
      setIsBusy(false)
    }
  }

  async function handleSave() {
    if (!content || changes.length === 0 || jsonError) return
    setIsBusy(true)
    setStatus('Saving changes...')
    setStatusKind('neutral')
    try {
      const response = await fetch(`${API_BASE_URL}/api/cv`, {
        method: 'PUT', headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' }, body: JSON.stringify(content),
      })
      if (response.status === 401 || response.status === 403) {
        signOut()
        throw new Error('Your session expired. Sign in again.')
      }
      if (!response.ok) {
        const result = await response.json().catch(() => null)
        throw new Error(result?.error ?? `Save failed with ${response.status}`)
      }
      const result = await response.json()
      setContent(result)
      setSavedContent(structuredClone(result))
      setJsonDraft(JSON.stringify(result, null, 2))
      setStatus('Changes saved')
      setStatusKind('success')
    } catch (error) {
      setStatus(error.message)
      setStatusKind('error')
    } finally {
      setIsBusy(false)
    }
  }

  function updateContent(nextContent) {
    setContent(nextContent)
    setJsonDraft(JSON.stringify(nextContent, null, 2))
    setJsonError('')
    setStatus('Unsaved changes')
    setStatusKind('neutral')
  }

  function updateJson(value) {
    setJsonDraft(value)
    try {
      const parsed = JSON.parse(value)
      setContent(parsed)
      setJsonError('')
      setStatus('Unsaved changes')
      setStatusKind('neutral')
    } catch {
      setJsonError('Fix the JSON syntax before saving or returning to the form.')
    }
  }

  function changeMode(mode) {
    if (mode === 'form' && jsonError) return
    if (mode === 'json' && content) setJsonDraft(JSON.stringify(content, null, 2))
    setEditorMode(mode)
  }

  function signOut() {
    sessionStorage.removeItem(TOKEN_KEY)
    setToken('')
    setContent(null)
    setSavedContent(null)
  }

  if (!token) {
    return (
      <main className="admin-page admin-login-page">
        <form className="admin-login" onSubmit={handleLogin}>
          <p className="admin-kicker">OnlineCV</p><h1>Administrator sign in</h1>
          <label>Username<input autoComplete="username" onChange={(event) => setUsername(event.target.value)} required value={username} /></label>
          <label>Password<input autoComplete="current-password" onChange={(event) => setPassword(event.target.value)} required type="password" value={password} /></label>
          <button disabled={isBusy} type="submit">Sign in</button>
          {status && <p className={`admin-status admin-status--${statusKind}`}>{status}</p>}
          <a href="/">Return to CV</a>
        </form>
      </main>
    )
  }

  const [sectionTitle, sectionDescription] = SECTION_META[selectedSection] ?? [humanize(selectedSection), 'Edit this CV section.']
  return (
    <main className="admin-page admin-editor-page">
      <header className="admin-header">
        <div><p className="admin-kicker">OnlineCV</p><h1>Content editor</h1></div>
        <div className="admin-header-actions">
          <a href="/" target="_blank" rel="noreferrer">View CV</a>
          <button className="admin-button-secondary" onClick={signOut} type="button">Sign out</button>
        </div>
      </header>

      <div className="admin-shell">
        <aside className="admin-sidebar">
          <label className="admin-section-search"><span>Find a section</span><input onChange={(event) => setSectionFilter(event.target.value)} placeholder="Search sections" type="search" value={sectionFilter} /></label>
          <nav aria-label="CV sections">
            {availableSections.map((key) => {
              const label = SECTION_META[key]?.[0] ?? humanize(key)
              const sectionChanges = changes.filter((change) => change.path[0] === key).length
              return <button className={selectedSection === key ? 'is-active' : ''} key={key} onClick={() => setSelectedSection(key)} type="button"><span>{label}</span>{sectionChanges > 0 && <strong>{sectionChanges}</strong>}</button>
            })}
          </nav>
        </aside>

        <section className="admin-workspace" aria-label="CV content editor">
          <div className="admin-toolbar">
            <div className="admin-toolbar-actions">
              <button disabled={isBusy || changes.length === 0 || Boolean(jsonError)} onClick={handleSave} type="button">Save {changes.length > 0 ? `${changes.length} change${changes.length === 1 ? '' : 's'}` : 'changes'}</button>
              <button className="admin-button-secondary" disabled={isBusy} onClick={() => loadContent()} type="button">Reload</button>
            </div>
            <div className="admin-mode" aria-label="Editor mode">
              <button className={editorMode === 'form' ? 'is-active' : ''} disabled={Boolean(jsonError)} onClick={() => changeMode('form')} type="button">Form</button>
              <button className={editorMode === 'json' ? 'is-active' : ''} onClick={() => changeMode('json')} type="button">Advanced JSON</button>
            </div>
            <p className={`admin-status admin-status--${statusKind}`} aria-live="polite">{status}</p>
          </div>

          {!content ? <div className="admin-loading">Loading CV content...</div> : editorMode === 'json' ? (
            <div className="admin-json-editor">
              <div><h2>Advanced JSON</h2><p>Edit the complete data document directly.</p></div>
              {jsonError && <p className="admin-json-error">{jsonError}</p>}
              <textarea aria-label="CV content JSON" onChange={(event) => updateJson(event.target.value)} spellCheck="false" value={jsonDraft} />
            </div>
          ) : (
            <div className="admin-form-editor">
              <div className="admin-section-heading"><p>CV section</p><h2>{sectionTitle}</h2><span>{sectionDescription}</span></div>
              <FieldEditor fieldKey={selectedSection} onChange={updateContent} path={[selectedSection]} source={content} value={content[selectedSection]} />
            </div>
          )}
        </section>

        <aside className="admin-changes" aria-label="Unsaved changes">
          <div className="admin-changes-heading"><div><p>Review</p><h2>Unsaved changes</h2></div><strong>{changes.length}</strong></div>
          {changes.length === 0 ? <p className="admin-empty">Your edits will appear here before you save them.</p> : (
            <ol>{changes.slice(0, 30).map((change, index) => <li key={`${formatPath(change.path)}-${index}`}><strong>{formatPath(change.path)}</strong><div><span>{formatValue(change.before)}</span><b>to</b><span>{formatValue(change.after)}</span></div></li>)}</ol>
          )}
          {changes.length > 30 && <p className="admin-change-overflow">And {changes.length - 30} more changes.</p>}
        </aside>
      </div>
    </main>
  )
}

export default Admin

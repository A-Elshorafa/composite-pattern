import { useEffect, useState } from 'react'
import { addApartment, addRoom, createBuilding, getBuilding, listBuildings, releaseRoom, rentRoom } from './api.js'

const money = (n) => Number(n).toLocaleString(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })

const inputCls =
  'w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-base focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-200'
const btnCls =
  'rounded-lg bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-700 disabled:opacity-50'
const ghostCls =
  'rounded-lg border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-50'

function Totals({ node }) {
  return (
    <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-slate-600">
      <span><b>{node.availableRooms}</b> / {node.totalRooms} rooms available</span>
      <span>Income <b>{money(node.currentMonthlyIncome)}</b> / {money(node.potentialMonthlyIncome)}</span>
    </div>
  )
}

function Room({ room, onRent, onRelease, busy }) {
  const [tenant, setTenant] = useState('')
  const rented = room.tenant != null
  return (
    <li className="flex flex-col gap-2 rounded-lg bg-slate-50 p-3 ring-1 ring-slate-200 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <span className="mr-2 text-xs text-slate-400">#{room.id}</span>
        <span className="font-medium text-slate-800">{room.name}</span>
        <span className="ml-2 text-sm text-slate-500">{money(room.monthlyPrice)}/mo</span>
        <span className={`ml-2 rounded-full px-2 py-0.5 text-xs font-medium ${rented ? 'bg-rose-100 text-rose-700' : 'bg-emerald-100 text-emerald-700'}`}>
          {rented ? `Rented by ${room.tenant}` : 'Available'}
        </span>
      </div>
      {rented ? (
        <button className={ghostCls} disabled={busy} onClick={() => onRelease(room.id)}>Release</button>
      ) : (
        <form className="flex gap-2" onSubmit={(e) => { e.preventDefault(); onRent(room.id, tenant).then(() => setTenant('')) }}>
          <input className={inputCls + ' sm:w-40'} placeholder="Tenant name" value={tenant} onChange={(e) => setTenant(e.target.value)} required />
          <button className={btnCls} disabled={busy}>Rent</button>
        </form>
      )}
    </li>
  )
}

function Apartment({ apt, actions, busy }) {
  const [name, setName] = useState('')
  const [price, setPrice] = useState('300')
  return (
    <div className="rounded-xl bg-white p-4 shadow-sm ring-1 ring-slate-200">
      <div className="mb-3">
        <h3 className="font-semibold text-slate-800"><span className="mr-2 text-xs font-normal text-slate-400">Apartment #{apt.id}</span>{apt.name}</h3>
        <Totals node={apt} />
      </div>
      <ul className="space-y-2">
        {apt.children.map((r) => <Room key={r.id} room={r} busy={busy} onRent={actions.rent} onRelease={actions.release} />)}
      </ul>
      <form className="mt-3 flex flex-col gap-2 sm:flex-row" onSubmit={(e) => { e.preventDefault(); actions.addRoom(apt.id, name, Number(price)).then(() => setName('')) }}>
        <input className={inputCls} placeholder="New room name" value={name} onChange={(e) => setName(e.target.value)} required />
        <input className={inputCls + ' sm:w-32'} type="number" min="1" placeholder="Price" value={price} onChange={(e) => setPrice(e.target.value)} required />
        <button className={ghostCls} disabled={busy}>+ Room</button>
      </form>
    </div>
  )
}

export default function App() {
  const [buildings, setBuildings] = useState([])
  const [selected, setSelected] = useState(null)
  const [tree, setTree] = useState(null)
  const [toast, setToast] = useState(null)
  const [busy, setBusy] = useState(false)
  const [bName, setBName] = useState('')
  const [bAddress, setBAddress] = useState('')
  const [aptName, setAptName] = useState('')

  const notify = (kind, msg) => {
    setToast({ kind, msg })
    setTimeout(() => setToast(null), 5000)
  }

  const refresh = async (id = selected) => {
    const list = await listBuildings()
    setBuildings(list)
    const current = id ?? list[0]?.id ?? null
    setSelected(current)
    setTree(current ? await getBuilding(current) : null)
  }

  // Runs an API call, then reloads the whole tree so every level shows fresh totals.
  const run = async (fn, buildingId) => {
    setBusy(true)
    try {
      const result = await fn()
      await refresh(buildingId ?? (typeof result === "number" ? result : undefined))
    } catch (e) {
      notify('error', e.message === 'Failed to fetch' ? 'Cannot reach the backend on :8095 — is it running?' : e.message)
    } finally { setBusy(false) }
  }

  useEffect(() => { run(async () => {}) }, [])

  const actions = {
    rent: (id, tenant) => run(() => rentRoom(id, tenant)),
    release: (id) => run(() => releaseRoom(id)),
    addRoom: (aptId, name, price) => run(() => addRoom(aptId, name, price)),
  }

  return (
    <div className="mx-auto max-w-4xl space-y-4 p-3 sm:p-6">
      <header>
        <h1 className="text-2xl font-bold text-slate-900">Composite Pattern · Apartment Rentals</h1>
        <p className="text-sm text-slate-600">Building → Apartments → Rooms. Rent a room and watch the totals change at every level.</p>
      </header>

      {toast && <div className={`rounded-lg px-4 py-3 text-sm ring-1 ${toast.kind === 'error' ? 'bg-rose-50 text-rose-700 ring-rose-200' : 'bg-emerald-50 text-emerald-700 ring-emerald-200'}`}>{toast.msg}</div>}

      <section className="rounded-2xl bg-white p-4 shadow-sm ring-1 ring-slate-200 sm:p-6">
        <div className="flex flex-wrap gap-2">
          {buildings.map((b) => (
            <button key={b.id} onClick={() => run(async () => {}, b.id)}
              className={`rounded-lg px-3 py-2 text-sm font-medium ${b.id === selected ? 'bg-indigo-600 text-white' : 'bg-slate-100 text-slate-700 hover:bg-slate-200'}`}>
              {b.name}
            </button>
          ))}
        </div>
        <form className="mt-4 flex flex-col gap-2 sm:flex-row" onSubmit={(e) => {
          e.preventDefault()
          run(async () => { const b = await createBuilding(bName, bAddress); setBName(''); setBAddress(''); return b.id })
        }}>
          <input className={inputCls} placeholder="New building name" value={bName} onChange={(e) => setBName(e.target.value)} required />
          <input className={inputCls} placeholder="Address" value={bAddress} onChange={(e) => setBAddress(e.target.value)} required />
          <button className={ghostCls} disabled={busy}>+ Building</button>
        </form>
      </section>

      {tree && (
        <section className="space-y-3 rounded-2xl bg-white p-4 shadow-sm ring-1 ring-slate-200 sm:p-6">
          <div>
            <h2 className="text-lg font-semibold text-slate-800"><span className="mr-2 text-xs font-normal text-slate-400">Building #{tree.id}</span>{tree.name}</h2>
            <Totals node={tree} />
          </div>
          <div className="space-y-3">
            {tree.children.map((a) => <Apartment key={a.id} apt={a} actions={actions} busy={busy} />)}
          </div>
          <form className="flex flex-col gap-2 sm:flex-row" onSubmit={(e) => { e.preventDefault(); run(() => addApartment(tree.id, aptName), tree.id).then(() => setAptName('')) }}>
            <input className={inputCls} placeholder="New apartment name" value={aptName} onChange={(e) => setAptName(e.target.value)} required />
            <button className={ghostCls} disabled={busy}>+ Apartment</button>
          </form>
        </section>
      )}
    </div>
  )
}

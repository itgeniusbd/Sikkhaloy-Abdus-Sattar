import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { useEffect } from 'react'
import Layout from './components/Layout.jsx'
import Home from './pages/Home.jsx'
import Gallery from './pages/Gallery.jsx'
import About from './pages/About.jsx'
import Blog from './pages/Blog.jsx'
import BlogDetail from './pages/BlogDetail.jsx'
import EduNotice from './pages/EduNotice.jsx'
import CmsPage from './pages/CmsPage.jsx'
import ApplyOnline from './pages/ApplyOnline.jsx'
import Career from './pages/Career.jsx'
import CareerDetail from './pages/CareerDetail.jsx'
import Faculty from './pages/Faculty.jsx'
import Videos from './pages/Videos.jsx'
import Contact from './pages/Contact.jsx'
import Login from './pages/Login.jsx'
import Admin from './pages/Admin.jsx'
import { useApp } from './context/AppContext.jsx'

function ScrollToTop() {
  const { pathname } = useLocation()
  useEffect(() => {
    window.scrollTo(0, 0)
  }, [pathname])
  return null
}

function Protected({ children }) {
  const { authed } = useApp()
  if (!authed) return <Navigate to="/login" replace />
  return children
}

export default function App() {
  return (
    <>
      <ScrollToTop />
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Home />} />
          <Route path="/gallery" element={<Gallery />} />
          <Route path="/videos" element={<Videos />} />
          <Route path="/notices" element={<EduNotice />} />
          <Route path="/notices/:id" element={<EduNotice />} />
          <Route path="/notice" element={<Blog />} />
          <Route path="/notice/:id" element={<BlogDetail />} />
          <Route path="/events" element={<Blog category="Events" eyebrowKey="pages.eventsEyebrow" titleKey="pages.eventsTitle" emptyKey="pages.noEvents" />} />
          <Route path="/events/:id" element={<BlogDetail backTo="/events" />} />
          <Route path="/blog" element={<Blog />} />
          <Route path="/blog/:id" element={<BlogDetail />} />
          <Route path="/about" element={<About />} />
          <Route path="/teachers" element={<Faculty />} />
          <Route path="/apply-online" element={<ApplyOnline />} />
          <Route path="/career" element={<Career />} />
          <Route path="/career/:id" element={<CareerDetail />} />
          <Route path="/p/:slug" element={<CmsPage />} />
          <Route path="/contact" element={<Contact />} />
          <Route path="/login" element={<Login />} />
          <Route path="/project" element={<Navigate to="/gallery" replace />} />
          <Route path="/project/:slug" element={<Navigate to="/gallery" replace />} />
        </Route>
        <Route
          path="/admin"
          element={
            <Protected>
              <Admin />
            </Protected>
          }
        />
      </Routes>
    </>
  )
}

import PeopleGrid from '../components/PeopleGrid.jsx'
import { useApp } from '../context/AppContext.jsx'
import { useLang } from '../context/LanguageContext.jsx'

export default function Faculty() {
  const { teachers, staff } = useApp()
  const { t } = useLang()

  return (
    <section className="page-pad">
      <div className="container">
        <p className="eyebrow">{t('pages.facultyEyebrow')}</p>
        <h1 className="page-title">{t('pages.facultyTitle')}</h1>
        <p className="lead">{t('pages.facultyLead')}</p>

        <div className="faculty-block">
          <h2 className="faculty-heading">{t('pages.facultyTeachers')}</h2>
          <PeopleGrid people={teachers} emptyLabel={t('pages.noFacultyTeachers')} />
        </div>

        <div className="faculty-block">
          <h2 className="faculty-heading">{t('pages.facultyStaff')}</h2>
          <PeopleGrid people={staff} emptyLabel={t('pages.noFacultyStaff')} />
        </div>
      </div>
    </section>
  )
}

import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { getUser } from '../api/api';
import { useTranslation } from 'react-i18next';
import { handleError } from '../helpers/ErrorHandler';

export default function UserProfile() {
  const { id } = useParams();
  const [user, setUser] = useState(null);

  const navigate = useNavigate();

  useEffect(() => {
    getUser(id)
      .then((res) => {
        if (res) {
          const userObj = {
            username: res?.data.username,
            email: res?.data.email,
            id: res?.data.id,
          };
          setUser(userObj);
        }
      })
      .catch((e) => {
        handleError(e);
        navigate('/');
      });
  }, []);

  return (
    <>
      <h1>
        {user?.id} - {user?.username}
      </h1>
      <p>Email: {user?.email}</p>
    </>
  );
}

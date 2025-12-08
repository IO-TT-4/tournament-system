import axios from 'axios';
import i18next from 'i18next';
import { toast } from 'react-toastify';

export function handleError(error) {
  if (axios.isAxiosError(error)) {
    const theme = localStorage.getItem('theme');
    toast.error(i18next.t('serverError', { ns: 'toast' }), { theme: theme });
  }
}

import axios from 'axios';
import i18next from 'i18next';
import { toast } from 'react-toastify';

export function handleError(error) {
  if (axios.isAxiosError(error)) {
    const theme = localStorage.getItem('theme');

    console.log(error?.response?.data?.code);

    if (error?.response?.data?.code == 'INVALID_CREDENTIALS') {
      toast.error(i18next.t('InvalidCredentials', { ns: 'toast' }), {
        theme: theme,
      });
    }

    toast.error(i18next.t('serverError', { ns: 'toast' }), { theme: theme });
  }
}

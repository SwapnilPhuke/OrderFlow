import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { AuthContext } from '../context/AuthContext';
import LoginPage from '../pages/LoginPage';

const renderLogin = (loginMock = jest.fn()) =>
  render(
    <AuthContext.Provider value={{
      login: loginMock, isAuthenticated: false,
      loadingUser: false, currentUser: null, isAdmin: false
    }}>
      <MemoryRouter><LoginPage /></MemoryRouter>
    </AuthContext.Provider>
  );

describe('LoginPage', () => {
  test('renders username, password inputs and submit button', () => {
    renderLogin();
    expect(screen.getByLabelText(/Username/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Sign In/i })).toBeInTheDocument();
  });

  test('calls login() with correct credentials on submit', async () => {
    const loginMock = jest.fn().mockResolvedValue({});
    renderLogin(loginMock);
    const user = userEvent.setup();

    await user.type(screen.getByLabelText(/Username/i), 'admin');
    await user.type(screen.getByLabelText(/Password/i), 'Admin@123456');
    await user.click(screen.getByRole('button', { name: /Sign In/i }));

    await waitFor(() =>
      expect(loginMock).toHaveBeenCalledWith('admin', 'Admin@123456')
    );
  });

  test('shows error alert when login() rejects', async () => {
    const loginMock = jest.fn().mockRejectedValue(new Error('Invalid credentials'));
    renderLogin(loginMock);
    const user = userEvent.setup();

    await user.type(screen.getByLabelText(/Username/i), 'bad');
    await user.type(screen.getByLabelText(/Password/i), 'wrong');
    await user.click(screen.getByRole('button', { name: /Sign In/i }));

    await waitFor(() =>
      expect(screen.getByRole('alert')).toBeInTheDocument()
    );
  });

  test('renders register link', () => {
    renderLogin();
    expect(screen.getByRole('link', { name: /Create one/i })).toBeInTheDocument();
  });
});

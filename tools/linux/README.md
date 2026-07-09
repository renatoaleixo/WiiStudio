# WiiStudio Linux helpers

## Polkit cache for Wiimms WWT

Raw WBFS devices normally require elevated access. The WiiStudio does not store
the user's password. On Linux, the safer approach is to install a Polkit action
for the exact `wwt` executable and let Polkit cache the authorization with
`auth_admin_keep`.

Install once:

```bash
tools/linux/install-wiistudio-polkit-policy.sh
```

If `wwt` is not in `PATH`, pass the executable path:

```bash
tools/linux/install-wiistudio-polkit-policy.sh /opt/wiimms/wwt
```

The script asks for authorization once through `pkexec`, writes:

```text
/usr/share/polkit-1/actions/com.wiistudio.wwt.policy
```

After that, calls like `pkexec /path/to/wwt ...` should reuse the authorization
while the desktop Polkit agent keeps it active. The exact cache duration is
controlled by the Linux distribution and Polkit agent.
